using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using Dapper;

namespace DapperExtensions
{
    public static class DapperExtensions
    {
        private readonly static object _lock = new object();

        private static Type _defaultMapper;
        private static Func<Type, ISqlGenerator, IDapperExtensionsImpl> _instanceFactory;
        private static IDapperExtensionsImpl _instance;
        private static ISqlDialect _sqlDialect;
        private static ISqlGenerator _sqlGenerator;
        
        /// <summary>
        /// Gets or sets the default class mapper to use when generating class maps. If not specified, AutoClassMapper<T> is used.
        /// </summary>
        public static Type DefaultMapper
        {
            get
            {
                return _defaultMapper;
            }

            set
            {
                _instance = null;
                _defaultMapper = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of sql to be generated.
        /// </summary>
        public static ISqlDialect SqlDialect
        {
            get
            {
                return _sqlDialect;
            }

            set
            {
                _instance = null;
                _sqlDialect = value;
                _sqlGenerator = new SqlGeneratorImpl(value);
            }
        }

        /// <summary>
        /// Gets the currently setup generator
        /// </summary>
        public static ISqlGenerator SqlGenerator
        {
            get { return _sqlGenerator; }
        }

        /// <summary>
        /// Get or sets the Dapper Extensions Implementation Factory.
        /// </summary>
        public static Func<Type, ISqlGenerator, IDapperExtensionsImpl> InstanceFactory
        {
            get
            {
                if (_instanceFactory == null)
                {
                    _instanceFactory = (dm, sg) => new DapperExtensionsImpl(dm, sg);
                }

                return _instanceFactory;
            }
            set
            {
                _instance = null;
                _instanceFactory = value;
            }
        }

        /// <summary>
        /// Gets the Dapper Extensions Implementation
        /// </summary>
        private static IDapperExtensionsImpl Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = InstanceFactory(DefaultMapper, _sqlGenerator);
                        }
                    }
                }

                return _instance;
            }
        }

        static DapperExtensions()
        {
            DefaultMapper = typeof(AutoClassMapper<>);
            SqlDialect = new SqlServerDialect();
        }

        /// <summary>
        /// Executes a query for the specified id, returning the data typed as per T
        /// </summary>
        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var result = Instance.Get<T>(connection, id, transaction, commandTimeout);
            return (T)result;
        }

        /// <summary>
        /// Executes an insert query for the specified entity.
        /// </summary>
        public static void Insert<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Instance.Insert<T>(connection, entities, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes an insert query for the specified entity, returning the primary key.  
        /// If the entity has a single key, just the value is returned.  
        /// If the entity has a composite key, an IDictionary&lt;string, object&gt; is returned with the key values.
        /// The key value for the entity will also be updated if the KeyType is a Guid or Identity.
        /// </summary>
        public static dynamic Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Insert<T>(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes an update query for the specified entity.
        /// </summary>
        public static bool Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Update<T>(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a delete query for the specified entity.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Delete<T>(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a delete query using the specified predicate.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, IPredicate predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Delete<T>(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// </summary>
        public static IEnumerable<T> GetList<T>(this IDbConnection connection, IPredicate predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return Instance.GetList<T>(connection, predicate, sort, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified page and resultsPerPage.
        /// </summary>
        public static IEnumerable<T> GetPage<T>(this IDbConnection connection, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return Instance.GetPage<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a query using the specified predicate, returning an integer that represents the number of rows that match the query.
        /// </summary>
        public static int Count<T>(this IDbConnection connection, IPredicate predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Count<T>(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        /// Gets the appropriate mapper for the specified type T. 
        /// If the mapper for the type is not yet created, a new mapper is generated from the mapper type specifed by DefaultMapper.
        /// </summary>
        public static IClassMapper GetMap<T>() where T : class
        {
            return Instance.GetMap<T>();
        }

        /// <summary>
        /// Clears the ClassMappers for each type.
        /// </summary>
        public static void ClearCache()
        {
            Instance.ClearCache();
        }

        /// <summary>
        /// Generates a COMB Guid which solves the fragmented index issue.
        /// See: http://davybrion.com/blog/2009/05/using-the-guidcomb-identifier-strategy
        /// </summary>
        public static Guid GetNextGuid()
        {
            return Instance.GetNextGuid();
        }
        
        private static string AppendStrings(this IEnumerable<string> list, string seperator = ", ")
        {
            return list.Aggregate(
                new StringBuilder(),
                (sb, s) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(s),
                sb => sb.ToString());
        }

        public interface IDapperExtensionsImpl
        {
            T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class;
            void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class;
            dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
            bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
            bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
            bool Delete<T>(IDbConnection connection, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class;
            IEnumerable<T> GetList<T>(IDbConnection connection, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
            IEnumerable<T> GetPage<T>(IDbConnection connection, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
            int Count<T>(IDbConnection connection, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class;
            IClassMapper GetMap<T>() where T : class;
            void ClearCache();
            Guid GetNextGuid();
        }

        public class DapperExtensionsImpl : IDapperExtensionsImpl
        {
            private readonly Type _defaultMapper;
            private readonly ISqlGenerator _sqlGenerator;
            private readonly List<Type> _simpleTypes;
            private readonly ConcurrentDictionary<Type, IClassMapper> _classMaps = new ConcurrentDictionary<Type, IClassMapper>();

            public DapperExtensionsImpl(Type defaultMapper, ISqlGenerator sqlGenerator)
            {
                _defaultMapper = defaultMapper;
                _sqlGenerator = sqlGenerator;
                _simpleTypes = new List<Type>
                               {
                                   typeof(byte),
                                   typeof(sbyte),
                                   typeof(short),
                                   typeof(ushort),
                                   typeof(int),
                                   typeof(uint),
                                   typeof(long),
                                   typeof(ulong),
                                   typeof(float),
                                   typeof(double),
                                   typeof(decimal),
                                   typeof(bool),
                                   typeof(string),
                                   typeof(char),
                                   typeof(Guid),
                                   typeof(DateTime),
                                   typeof(DateTimeOffset),
                                   typeof(byte[])
                               };
            }

            public T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                string sql = _sqlGenerator.Get(classMap);
                bool isSimpleType = IsSimpleType(id.GetType());
                IDictionary<string, object> paramValues = null;
                if (!isSimpleType)
                {
                    paramValues = ReflectionHelper.GetObjectValues(id);
                }

                DynamicParameters parameters = new DynamicParameters();
                var keys = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
                foreach (var key in keys)
                {
                    object value = id;
                    if (!isSimpleType)
                    {
                        value = paramValues[key.Name];
                    }

                    parameters.Add("@" + key.Name, value);
                }


                T result = connection.Query<T>(sql, parameters, transaction, true, commandTimeout, CommandType.Text).SingleOrDefault();
                return result;
            }

            public void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                var properties = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);

                foreach (var e in entities)
                {
                    foreach (var column in properties)
                    {
                        if (column.KeyType == KeyType.Guid)
                        {
                            Guid comb = GetNextGuid();
                            column.PropertyInfo.SetValue(e, comb, null);
                        }
                    }
                }

                string sql = _sqlGenerator.Insert(classMap);

                connection.Execute(sql, entities, transaction, commandTimeout, CommandType.Text);
            }

            public dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();

                foreach (var column in classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey))
                {
                    if (column.KeyType == KeyType.Guid)
                    {
                        Guid comb = GetNextGuid();
                        column.PropertyInfo.SetValue(entity, comb, null);
                    }
                }

                string sql = _sqlGenerator.Insert(classMap);
                connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
                IDictionary<string, object> keyValues = new ExpandoObject();

                foreach (var column in classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey))
                {
                    if (column.KeyType == KeyType.Identity)
                    {
                        string identitySql = _sqlGenerator.IdentitySql(classMap);
                        var identityId = connection.Query(identitySql, null, transaction, true, commandTimeout, CommandType.Text);
                        int id = (int)identityId.First().Id;
                        keyValues.Add(column.Name, id);
                        column.PropertyInfo.SetValue(entity, id, null);
                    }

                    if (column.KeyType == KeyType.Guid || column.KeyType == KeyType.Assigned)
                    {
                        keyValues.Add(column.Name, column.PropertyInfo.GetValue(entity, null));
                    }
                }

                if (keyValues.Count == 1)
                {
                    return keyValues.First().Value;
                }

                return keyValues;
            }

            public bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                string sql = _sqlGenerator.Update(classMap);
                return connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text) > 0;
            }

            public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                string sql = _sqlGenerator.Delete(classMap);
                return connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text) > 0;
            }

            public bool Delete<T>(IDbConnection connection, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string sql = _sqlGenerator.Delete(classMap, predicate, parameters);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
            }
            
            public IEnumerable<T> GetList<T>(IDbConnection connection, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string sql = _sqlGenerator.GetList(classMap, predicate, sort, parameters);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
            }

            public IEnumerable<T> GetPage<T>(IDbConnection connection, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string sql = _sqlGenerator.GetPage(classMap, predicate, sort, page, resultsPerPage, parameters);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
            }

            public int Count<T>(IDbConnection connection, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string sql = _sqlGenerator.Count(classMap, predicate, parameters);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                return (int)connection.Query(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text).Single().Total;
            }

            public IClassMapper GetMap<T>() where T : class
            {
                Type entityType = typeof(T);
                IClassMapper map;
                if (!_classMaps.TryGetValue(entityType, out map))
                {
                    Type[] types = entityType.Assembly.GetTypes();
                    Type mapType = (from type in types
                                    let interfaceType = type.GetInterface(typeof(IClassMapper<>).FullName)
                                    where interfaceType != null && interfaceType.GetGenericArguments()[0] == entityType
                                    select type).SingleOrDefault();

                    if (mapType == null)
                    {
                        mapType = _defaultMapper.MakeGenericType(typeof(T));
                    }

                    map = Activator.CreateInstance(mapType) as IClassMapper;
                    _classMaps[entityType] = map;
                }

                return map;
            }

            public void ClearCache()
            {
                _classMaps.Clear();
            }

            public Guid GetNextGuid()
            {
                byte[] b = Guid.NewGuid().ToByteArray();
                DateTime dateTime = new DateTime(1900, 1, 1);
                DateTime now = DateTime.Now;
                TimeSpan timeSpan = new TimeSpan(now.Ticks - dateTime.Ticks);
                TimeSpan timeOfDay = now.TimeOfDay;
                byte[] bytes1 = BitConverter.GetBytes(timeSpan.Days);
                byte[] bytes2 = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));
                Array.Reverse(bytes1);
                Array.Reverse(bytes2);
                Array.Copy(bytes1, bytes1.Length - 2, b, b.Length - 6, 2);
                Array.Copy(bytes2, bytes2.Length - 4, b, b.Length - 4, 4);
                return new Guid(b);
            }

            private bool IsSimpleType(Type type)
            {
                Type actualType = type;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    actualType = type.GetGenericArguments()[0];
                }

                return _simpleTypes.Contains(actualType);
            }
        }

        public interface ISqlGenerator
        {
            string Get(IClassMapper classMap);
            string Insert(IClassMapper classMap);
            string Update(IClassMapper classMap);
            string Delete(IClassMapper classMap);
            string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters);
            string GetList(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters);
            string GetPage(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters);
            string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters);
            string IdentitySql(IClassMapper classMap);
            string GetTableName(IClassMapper map);
            string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias);
            string GetColumnName(IClassMapper map, string propertyName, bool includeAlias);
        }

        public class SqlGeneratorImpl : ISqlGenerator
        {
            private readonly ISqlDialect _dialect;

            public SqlGeneratorImpl(ISqlDialect dialect)
            {
                _dialect = dialect;
            }

            public string Get(IClassMapper classMap)
            {
                if (!classMap.Properties.Any(c => c.KeyType != KeyType.NotAKey))
                {
                    throw new ArgumentException("At least one Key column must be defined.");
                }

                return string.Format("SELECT {0} FROM {1} WHERE {2}",
                    BuildSelectColumns(classMap),
                    GetTableName(classMap),
                    BuildWhere(classMap));
            }

            public string Insert(IClassMapper classMap)
            {
                if (classMap.Properties.Count(c => c.KeyType == KeyType.Identity) > 1)
                {
                    throw new ArgumentException("Can only set 1 property to Identity.");
                }

                var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity));
                var columnNames = columns.Select(p => GetColumnName(classMap, p, false));
                var parameters = columns.Select(p => "@" + p.Name);

                return string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                     GetTableName(classMap),
                                     columnNames.AppendStrings(),
                                     parameters.AppendStrings());
            }

            public string Update(IClassMapper classMap)
            {
                if (!classMap.Properties.Any(c => c.KeyType != KeyType.NotAKey))
                {
                    throw new ArgumentException("At least one Key column must be defined.");
                }

                var columns = classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity));
                var setSql = columns.Select(p => GetColumnName(classMap, p, false) + " = @" + p.Name);
                return string.Format("UPDATE {0} SET {1} WHERE {2}",
                    GetTableName(classMap),
                    setSql.AppendStrings(),
                    BuildWhere(classMap));
            }

            public string Delete(IClassMapper classMap)
            {
                if (!classMap.Properties.Any(c => c.KeyType != KeyType.NotAKey))
                {
                    throw new ArgumentException("At least one Key column must be defined.");
                }

                return string.Format("DELETE FROM {0} WHERE {1}",
                    GetTableName(classMap),
                    BuildWhere(classMap));
            }

            public string Delete(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
            {
                StringBuilder sql = new StringBuilder(string.Format("DELETE FROM {0}", GetTableName(classMap)));
                if (predicate != null)
                {
                    sql.Append(" WHERE ")
                        .Append(predicate.GetSql(parameters));
                }

                return sql.ToString();
            }

            public string GetList(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDictionary<string, object> parameters)
            {
                StringBuilder sql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                    BuildSelectColumns(classMap),
                    GetTableName(classMap)));
                if (predicate != null)
                {
                    sql.Append(" WHERE ")
                        .Append(predicate.GetSql(parameters));
                }

                if (sort != null && sort.Any())
                {
                    sql.Append(" ORDER BY ")
                        .Append(sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings());
                }

                return sql.ToString();
            }

            public string GetPage(IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDictionary<string, object> parameters)
            {
                if (sort == null || !sort.Any())
                {
                    throw new ArgumentException("Sort must be supplied for GetPage.");
                }

                StringBuilder innerSql = new StringBuilder(string.Format("SELECT {0} FROM {1}",
                    BuildSelectColumns(classMap),
                    GetTableName(classMap)));
                if (predicate != null)
                {
                    innerSql.Append(" WHERE ")
                        .Append(predicate.GetSql(parameters));
                }

                string orderBy = sort.Select(s => GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC")).AppendStrings();
                innerSql.Append(" ORDER BY " + orderBy);
                
                string sql = _dialect.GetPagingSql(innerSql.ToString(), page, resultsPerPage, parameters);
                return sql;
            }

            public string Count(IClassMapper classMap, IPredicate predicate, IDictionary<string, object> parameters)
            {
                StringBuilder sql = new StringBuilder(string.Format("SELECT COUNT(*) AS [Total] FROM {0}",
                                    GetTableName(classMap)));
                if (predicate != null)
                {
                    sql.Append(" WHERE ")
                        .Append(predicate.GetSql(parameters));
                }

                return sql.ToString();
            }

            public string IdentitySql(IClassMapper classMap)
            {
                return _dialect.GetIdentitySql(GetTableName(classMap));
            }

            public string GetTableName(IClassMapper map)
            {
                return _dialect.GetTableName(map.SchemaName, map.TableName, null);
            }

            public string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias)
            {
                string alias = null;
                if (property.ColumnName != property.Name && includeAlias)
                {
                    alias = property.Name;
                }

                return _dialect.GetColumnName(GetTableName(map), property.ColumnName, alias);
            }

            public string GetColumnName(IClassMapper map, string propertyName, bool includeAlias)
            {
                IPropertyMap propertyMap = map.Properties.SingleOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
                if (propertyMap == null)
                {
                    throw new ArgumentException(string.Format("Could not find '{0}' in Mapping.", propertyName));
                }

                return GetColumnName(map, propertyMap, includeAlias);
            }

            private string BuildSelectColumns(IClassMapper classMap)
            {
                var columns = classMap.Properties.Where(p => !p.Ignored).Select(p => GetColumnName(classMap, p, true));
                return columns.AppendStrings();
            }

            private string BuildWhere(IClassMapper classMap)
            {
                var where = classMap.Properties
                    .Where(p => p.KeyType != KeyType.NotAKey)
                    .Select(p => GetColumnName(classMap, p, false) + " = @" + p.Name);
                return where.AppendStrings(" AND ");
            }
        }
    }
}
