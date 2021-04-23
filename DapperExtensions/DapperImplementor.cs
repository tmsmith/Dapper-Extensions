using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DapperExtensions
{
    public interface IDapperImplementor
    {
        ISqlGenerator SqlGenerator { get; }
        string LastExecutedCommand { get; }
        T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class;
        void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class;
        dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
        bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties) where T : class;
        bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
        bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class;
        IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class;        
        IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class;
        IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class;
        int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class;
        IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout);
    }

    public class DapperImplementor : IDapperImplementor
    {
        public DapperImplementor(ISqlGenerator sqlGenerator)
        {
            SqlGenerator = sqlGenerator;
        }

        public ISqlGenerator SqlGenerator { get; private set; }
        public string LastExecutedCommand { get; private set; }

        public T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate predicate = GetIdPredicate(classMap, id);
            T result = GetList<T>(connection, classMap, predicate, null, transaction, commandTimeout, true).SingleOrDefault();
            return result;
        }

        public void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            var notKeyProperties = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
            var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);

            var parameters = new List<DynamicParameters>();

            foreach (var e in entities)
            {
                foreach (var column in notKeyProperties)
                {
                    if (column.KeyType == KeyType.Guid && (Guid)column.GetValue(e) == Guid.Empty)
                    {
                        Guid comb = SqlGenerator.Configuration.GetNextGuid();
                        column.SetValue(e, comb);
                    }
                }

                var dynamicParameters = new DynamicParameters();
                foreach (
                    var prop in
                        e.GetType()
                         .GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                         .Where(p => triggerIdentityColumn == null || p.Name != triggerIdentityColumn.Name)
                    )
                {
                    var propValue = prop.GetValue(e, null);

                    var parameter = ReflectionHelper.GetParameter(typeof (T), SqlGenerator, prop.Name, propValue);
                    dynamicParameters.Add(parameter.Name, parameter.Value, parameter.DbType,
                                          parameter.ParameterDirection, parameter.Size, parameter.Precision,
                                          parameter.Scale);
                }

                if (triggerIdentityColumn != null)
                {
                    // defaultValue need for identify type of parameter
                    var defaultValue = typeof(T).GetProperty(triggerIdentityColumn.Name).GetValue(e, null);
                    dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);
                }

                parameters.Add(dynamicParameters);
            }

            string sql = SqlGenerator.Insert(classMap);
            LastExecutedCommand = sql;
            connection.Execute(sql, parameters, transaction, commandTimeout, CommandType.Text);
        }

        public dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            List<IMemberMap> nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
            var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
            var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);
            foreach (var column in nonIdentityKeyProperties)
            {
                if (column.KeyType == KeyType.Guid && (Guid)column.GetValue(entity) == Guid.Empty)
                {
                    Guid comb = SqlGenerator.Configuration.GetNextGuid();
                    column.SetValue(entity, comb);
                }
            }

            var dynamicParameters = new DynamicParameters();
            foreach (var prop in entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                .Where(p => triggerIdentityColumn == null || p.Name != triggerIdentityColumn.Name))
            {
                var propValue = prop.GetValue(entity, null);
                var parameter = ReflectionHelper.GetParameter(typeof (T), SqlGenerator, prop.Name, propValue);
                dynamicParameters.Add(parameter.Name, parameter.Value, parameter.DbType,
                                      parameter.ParameterDirection, parameter.Size, parameter.Precision,
                                      parameter.Scale);
            }

            IDictionary<string, object> keyValues = new ExpandoObject();
            string sql = SqlGenerator.Insert(classMap);

            LastExecutedCommand = sql;
            if (identityColumn != null)
            {
                IEnumerable<long> result;
                if (SqlGenerator.SupportsMultipleStatements())
                {
                    sql += SqlGenerator.Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(classMap);
                    LastExecutedCommand = sql;
                    result = connection.Query<long>(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text);
                }
                else
                {
                    connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
                    sql = SqlGenerator.IdentitySql(classMap);
                    LastExecutedCommand += string.Format("{0}{0}{1}", Environment.NewLine, sql);
                    result = connection.Query<long>(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text);
                }

                // We are only interested in the first identity, but we are iterating over all resulting items (if any).
                // This makes sure that ADO.NET drivers (like MySql) won't actively terminate the query.
                bool hasResult = false;
                long identity = 0;
                foreach (var identityValue in result)
                {
                    if (hasResult)
                    {
                        continue;
                    }
                    identity = identityValue;
                    hasResult = true;
                }
                if (!hasResult)
                {
                    throw new InvalidOperationException("The source sequence is empty.");
                }

                keyValues.Add(identityColumn.Name, identity);
                identityColumn.SetValue(entity, Convert.ChangeType(identity, identityColumn.MemberType));
            }
            else if (triggerIdentityColumn != null)
            {
                dynamicParameters = new DynamicParameters();
                foreach (var prop in entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.Name != triggerIdentityColumn.Name))
                {
                    dynamicParameters.Add(prop.Name, prop.GetValue(entity, null));
                }

                // defaultValue need for identify type of parameter
                var defaultValue = entity.GetType().GetProperty(triggerIdentityColumn.Name).GetValue(entity, null);
                dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);

                connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
                var value = dynamicParameters.Get<object>(SqlGenerator.Configuration.Dialect.ParameterPrefix + "IdOutParam");
                keyValues.Add(triggerIdentityColumn.Name, value);
                triggerIdentityColumn.SetValue(entity, value);
            }
            else
            {
                connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
            }

            foreach (var column in nonIdentityKeyProperties)
            {
                keyValues.Add(column.Name, column.GetValue(entity));
            }

            if (keyValues.Count == 1)
            {
                return keyValues.First().Value;
            }

            return keyValues;
        }

        public bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties = false) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate predicate = GetKeyPredicate<T>(classMap, entity);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Update(classMap, predicate, parameters, ignoreAllKeyProperties);

            var columns = ignoreAllKeyProperties 
                ? classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly) && p.KeyType == KeyType.NotAKey) 
                : classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.TriggerIdentity || p.KeyType == KeyType.Assigned));

            DynamicParameters dynamicParameters = GetDynamicParameters(parameters);

            foreach (var property in ReflectionHelper.GetObjectValues(entity).Where(property => columns.Any(c => c.Name == property.Key)))
            {
                var parameter = ReflectionHelper.GetParameter(typeof (T), SqlGenerator, property.Key, property.Value);
                dynamicParameters.Add(parameter.Name, parameter.Value, parameter.DbType,
                                      parameter.ParameterDirection, parameter.Size, parameter.Precision,
                                      parameter.Scale);
            }

            

            LastExecutedCommand = sql;
            return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate predicate = GetKeyPredicate<T>(classMap, entity);
            return Delete<T>(connection, classMap, predicate, transaction, commandTimeout);
        }

        public bool Delete<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            return Delete<T>(connection, classMap, wherePredicate, transaction, commandTimeout);
        }

        public IEnumerable<T> GetList<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            return GetList<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout, buffered, projections);
        }

        public IEnumerable<T> GetPage<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            return GetPage<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        public IEnumerable<T> GetSet<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            return GetSet<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
        }

        public int Count<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            IClassMapper classMap = SqlGenerator.Configuration.GetMap<T>();
            IPredicate wherePredicate = GetPredicate(classMap, predicate);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Count(classMap, wherePredicate, parameters);
            DynamicParameters dynamicParameters = GetDynamicParameters(parameters);

            LastExecutedCommand = sql;
            return (int)connection.Query(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text).Single().Total;
        }

        public IMultipleResultReader GetMultiple(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
        {
            if (SqlGenerator.SupportsMultipleStatements())
            {
                return GetMultipleByBatch(connection, predicate, transaction, commandTimeout);
            }

            return GetMultipleBySequence(connection, predicate, transaction, commandTimeout);
        }

        protected IEnumerable<T> GetList<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> projections = null) where T : class
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Select(classMap, predicate, sort, parameters, projections);
            DynamicParameters dynamicParameters = GetDynamicParameters(parameters);

            LastExecutedCommand = sql;
            return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        protected IEnumerable<T> GetPage<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters);
            DynamicParameters dynamicParameters = GetDynamicParameters(parameters);

            LastExecutedCommand = sql;
            return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        protected IEnumerable<T> GetSet<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters);
            DynamicParameters dynamicParameters = GetDynamicParameters(parameters);

            LastExecutedCommand = sql;
            return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        protected bool Delete<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Delete(classMap, predicate, parameters);
            DynamicParameters dynamicParameters = GetDynamicParameters(parameters);

            LastExecutedCommand = sql;
            return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
        }

        protected IPredicate GetPredicate(IClassMapper classMap, object predicate)
        {
            IPredicate wherePredicate = predicate as IPredicate;
            if (wherePredicate == null && predicate != null)
            {
                wherePredicate = GetEntityPredicate(classMap, predicate);
            }

            return wherePredicate;
        }

        protected IPredicate GetIdPredicate(IClassMapper classMap, object id)
        {
            bool isSimpleType = ReflectionHelper.IsSimpleType(id.GetType());
            var keys = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
            IDictionary<string, Func<object>> paramValues = null;
            IList<IPredicate> predicates = new List<IPredicate>();
            if (!isSimpleType)
            {
                paramValues = ReflectionHelper.GetObjectValues(id);
            }

            foreach (var key in keys)
            {
                object value = id;
                if (!isSimpleType)
                {
                    value = paramValues[key.Name]();
                }

                Type predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);

                IFieldPredicate fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = key.Name;
                fieldPredicate.Value = value;
                predicates.Add(fieldPredicate);
            }

            return predicates.Count == 1
                       ? predicates[0]
                       : new PredicateGroup
                       {
                           Operator = GroupOperator.And,
                           Predicates = predicates
                       };
        }

        protected IPredicate GetKeyPredicate<T>(IClassMapper classMap, T entity) where T : class
        {
            var whereFields = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
            if (!whereFields.Any())
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }

            IList<IPredicate> predicates = (from field in whereFields
                                            select new FieldPredicate<T>
                                            {
                                                Not = false,
                                                Operator = Operator.Eq,
                                                PropertyName = field.Name,
                                                Value = field.GetValue(entity)
                                            }).Cast<IPredicate>().ToList();

            return predicates.Count == 1
                       ? predicates[0]
                       : new PredicateGroup
                       {
                           Operator = GroupOperator.And,
                           Predicates = predicates
                       };
        }

        protected IPredicate GetEntityPredicate(IClassMapper classMap, object entity)
        {
            Type predicateType = typeof(FieldPredicate<>).MakeGenericType(classMap.EntityType);
            IList<IPredicate> predicates = new List<IPredicate>();
            var notIgnoredColumns = classMap.Properties.Where(p => !p.Ignored);
            foreach (var kvp in ReflectionHelper.GetObjectValues(entity).Where(property => notIgnoredColumns.Any(c => c.Name == property.Key)))
            {
                IFieldPredicate fieldPredicate = Activator.CreateInstance(predicateType) as IFieldPredicate;
                fieldPredicate.Not = false;
                fieldPredicate.Operator = Operator.Eq;
                fieldPredicate.PropertyName = kvp.Key;
                fieldPredicate.Value = kvp.Value();
                predicates.Add(fieldPredicate);
            }

            return predicates.Count == 1
                       ? predicates[0]
                       : new PredicateGroup
                       {
                           Operator = GroupOperator.And,
                           Predicates = predicates
                       };
        }

        protected GridReaderResultReader GetMultipleByBatch(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            StringBuilder sql = new StringBuilder();
            foreach (var item in predicate.Items)
            {
                IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
                IPredicate itemPredicate = item.Value as IPredicate;
                if (itemPredicate == null && item.Value != null)
                {
                    itemPredicate = GetPredicate(classMap, item.Value);
                }

                sql.AppendLine(SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters) + SqlGenerator.Configuration.Dialect.BatchSeperator);
            }

            DynamicParameters dynamicParameters = GetDynamicParameters(parameters);

            LastExecutedCommand = sql.ToString();
            SqlMapper.GridReader grid = connection.QueryMultiple(sql.ToString(), dynamicParameters, transaction, commandTimeout, CommandType.Text);
            return new GridReaderResultReader(grid);
        }

        protected SequenceReaderResultReader GetMultipleBySequence(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
        {
            IList<SqlMapper.GridReader> items = new List<SqlMapper.GridReader>();

            LastExecutedCommand = string.Empty;
            foreach (var item in predicate.Items)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                IClassMapper classMap = SqlGenerator.Configuration.GetMap(item.Type);
                IPredicate itemPredicate = item.Value as IPredicate;
                if (itemPredicate == null && item.Value != null)
                {
                    itemPredicate = GetPredicate(classMap, item.Value);
                }

                string sql = SqlGenerator.Select(classMap, itemPredicate, item.Sort, parameters);
                DynamicParameters dynamicParameters = GetDynamicParameters(parameters);

                LastExecutedCommand += string.Format("{0}{1}{1}", sql, Environment.NewLine);
                SqlMapper.GridReader queryResult = connection.QueryMultiple(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
                items.Add(queryResult);
            }

            return new SequenceReaderResultReader(items);
        }

        private static DynamicParameters GetDynamicParameters(Dictionary<string, object> parameters)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                if (parameter.Value is Parameter p)
                {
                    dynamicParameters.Add(p.Name, p.Value, p.DbType,
                                          p.ParameterDirection, p.Size, p.Precision,
                                          p.Scale);
                }
                else
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }
            }
            return dynamicParameters;
        }
    }
}
