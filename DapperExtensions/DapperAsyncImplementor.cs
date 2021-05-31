using Dapper;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper = Slapper.AutoMapper;

namespace DapperExtensions
{
    /// <summary>
    /// Interface for asyncImplementator
    /// </summary>
    public interface IDapperAsyncImplementor : IDapperImplementor
    {
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Get{T}"/>.
        /// </summary>
        Task<T> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, 
            IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
        /// </summary>
        Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetListAutoMap{T}"/>.
        /// </summary>
        Task<IEnumerable<T>> GetListAutoMapAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
        /// </summary>
        Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10, 
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPageAutoMap{T}"/>.
        /// </summary>
        Task<IEnumerable<T>> GetPageAutoMapAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
        /// </summary>
        Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Count{T}"/>.
        /// </summary>
        Task<int> CountAsync<T>(IDbConnection connection, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null, IList<IReferenceMap> includedProperties = null) where T : class;

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, IEnumerable{T}, IDbTransaction, int?)"/>.
        /// </summary>
        Task InsertAsync<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = default) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        Task<dynamic> InsertAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = default) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Update{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties = false, IList<IProjection> colsToUpdate = null) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, object, IDbTransaction, int?)"/>.
        /// </summary>
        Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class;

        Task<IMultipleResultReader> GetMultipleAsync(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout, IList<IReferenceMap> includedProperties = null);
    }

    public class DapperAsyncImplementor : DapperImplementor, IDapperAsyncImplementor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DapperAsyncImplementor"/> class.
        /// </summary>
        /// <param name="sqlGenerator">The SQL generator.</param>
        public DapperAsyncImplementor(ISqlGenerator sqlGenerator)
            : base(sqlGenerator) { }

        #region Implementation of IDapperAsyncImplementor
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, IEnumerable{T}, IDbTransaction, int?)"/>.
        /// </summary>
        public async Task InsertAsync<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = default) where T : class
        {
            //Got the information here to avoid doing it for each item and so we speed up the execution
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
            var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
            var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);
            var sequenceIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.SequenceIdentity).ToList();

            foreach (var e in entities)
                await InternalInsertAsync(connection, e, transaction, commandTimeout, classMap, nonIdentityKeyProperties, identityColumn, triggerIdentityColumn, sequenceIdentityColumn);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Insert{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        public async Task<dynamic> InsertAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            var classMap = SqlGenerator.Configuration.GetMap<T>();
            var nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
            var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
            var triggerIdentityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.TriggerIdentity);
            var sequenceIdentityColumn = classMap.Properties.Where(p => p.KeyType == KeyType.SequenceIdentity).ToList();

            return await InternalInsertAsync(connection, entity, transaction, commandTimeout, classMap, nonIdentityKeyProperties, identityColumn, triggerIdentityColumn, sequenceIdentityColumn);

            /*foreach (var column in nonIdentityKeyProperties)
            {
                if (column.KeyType == KeyType.Guid && (Guid)column.GetValue(entity) == Guid.Empty)
                {
                    var comb = SqlGenerator.Configuration.GetNextGuid();
                    column.SetValue(entity, comb);
                }
            }

            IDictionary<string, object> keyValues = new ExpandoObject();
            var sql = SqlGenerator.Insert(classMap);
            if (identityColumn != null)
            {
                IEnumerable<long> result;
                var dynamicParameters = GetDynamicParameters(classMap, entity, true);
                if (SqlGenerator.SupportsMultipleStatements())
                {
                    sql += SqlGenerator.Configuration.Dialect.BatchSeperator + SqlGenerator.IdentitySql(classMap);
                    result = connection.Query<long>(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text);
                }
                else
                {
                    connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
                    sql = SqlGenerator.IdentitySql(classMap);
                    result = connection.Query<long>(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text);
                }

                var identityValue = result.First();
                var identityInt = Convert.ToInt32(identityValue);
                keyValues.Add(identityColumn.Name, identityInt);
                identityColumn.SetValue(entity, identityInt);
            }
            else if (triggerIdentityColumn != null)
            {
                var dynamicParameters = new DynamicParameters();
                foreach (var prop in entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.Name != triggerIdentityColumn.Name))
                {
                    dynamicParameters.Add(prop.Name, prop.GetValue(entity, null));
                }

                // defaultValue need for identify type of parameter
                var defaultValue = entity.GetType().GetProperty(triggerIdentityColumn.Name).GetValue(entity, null);
                dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);

                await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false);

                var value = dynamicParameters.Get<object>(SqlGenerator.Configuration.Dialect.ParameterPrefix + "IdOutParam");
                keyValues.Add(triggerIdentityColumn.Name, value);
                triggerIdentityColumn.SetValue(entity, value);
            }
            else if (sequenceIdentityColumn.Count > 0)
            {
                if (sequenceIdentityColumn.Count > 1)
                    throw new ArgumentException("SequenceIdentity generator cannot be used with multi-column keys");

                var dynamicParameters = GetDynamicParameters(classMap, entity, true);
                var seqIdentityColumn = sequenceIdentityColumn[0];

                var query = $"select {seqIdentityColumn.SequenceName}.nextval seq from dual";
                var value = connection.ExecuteScalar<int>(query);

                keyValues.Add(seqIdentityColumn.Name, value);
                seqIdentityColumn.SetValue(entity, value);

                dynamicParameters.Add(seqIdentityColumn.Name, seqIdentityColumn.GetValue(entity));

                connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
            }
            else
            {
                await connection.ExecuteAsync(sql, entity, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false);
            }

            foreach (var column in nonIdentityKeyProperties)
            {
                keyValues.Add(column.Name, column.GetValue(entity));
            }

            if (keyValues.Count == 1)
            {
                return keyValues.First().Value;
            }

            return keyValues; */
        }
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Update{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        public async Task<bool> UpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties, IList<IProjection> colsToUpdate = null) where T : class
        {
            return await InternalUpdateAsync(connection, entity, transaction, colsToUpdate, commandTimeout, ignoreAllKeyProperties);

            /*GetMapAndPredicate<T>(entity, out var classMap, out var predicate, true);
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Update(classMap, predicate, parameters, ignoreAllKeyProperties, colsToUpdate);
            var dynamicParameters = GetDynamicParameters(classMap, entity);

            var columns = ignoreAllKeyProperties
                ? classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly) && p.KeyType == KeyType.NotAKey)
                : classMap.Properties.Where(p => !(p.Ignored || p.IsReadOnly || p.KeyType == KeyType.Identity || p.KeyType == KeyType.Assigned));

            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false) > 0; */
        }
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, T, IDbTransaction, int?)"/>.
        /// </summary>
        public async Task<bool> DeleteAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return await Task.FromResult(Delete<T>(connection, entity, transaction, commandTimeout));

            /*GetMapAndPredicate<T>(entity, out var classMap, out var predicate, true);
            return await DeleteAsync<T>(connection, classMap, predicate, transaction, commandTimeout).ConfigureAwait(false);*/
        }
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Delete{T}(IDbConnection, object, IDbTransaction, int?)"/>.
        /// </summary>
        public async Task<bool> DeleteAsync<T>(IDbConnection connection, object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return await Task.FromResult(Delete<T>(connection, predicate, transaction, commandTimeout));

            /*GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return await DeleteAsync<T>(connection, classMap, wherePredicate, transaction, commandTimeout).ConfigureAwait(false);*/
        }
        /*protected async Task<bool> DeleteAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Delete(classMap, predicate, parameters);
            var dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }
            return await connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false) > 0; 
        }*/
        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Get{T}"/>.
        /// </summary>
        public async Task<T> GetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, 
            IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await Task.FromResult(InternalGet<T>(connection, id, transaction, commandTimeout, colsToSelect, includedProperties));
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
        /// </summary>
        public async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, 
            int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await InternalGetListAutoMapAsync<T>(connection, predicate, sort, transaction, commandTimeout, buffered, colsToSelect, includedProperties);

            /*GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return await GetListAsync<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout, colsToSelect).ConfigureAwait(false); */
        }

        public async Task<IEnumerable<T>> GetListAutoMapAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, 
            bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await InternalGetListAutoMapAsync<T>(connection, predicate, sort, transaction, commandTimeout, buffered, colsToSelect, includedProperties);

            /*GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return GetListAutoMapAsync<T>(connection, classMap, wherePredicate, sort, transaction, commandTimeout, colsToSelect);*/
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
        /// </summary>
        public async Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10, 
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await InternalGetPageAutoMapAsync<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect, includedProperties);

            /*GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return await GetPageAsync<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout, colsToSelect).ConfigureAwait(false);*/
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPageAutoMap{T}"/>.
        /// </summary>
        public async Task<IEnumerable<T>> GetPageAutoMapAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1, int resultsPerPage = 10, 
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await InternalGetPageAutoMapAsync<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect, includedProperties);

            /*GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return await GetPageAutoMapAsync<T>(connection, classMap, wherePredicate, sort, page, resultsPerPage, transaction, commandTimeout, colsToSelect).ConfigureAwait(false);*/
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
        /// </summary>
        public async Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, 
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await InternalGetSetAsync<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, colsToSelect, includedProperties);

            /*GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            return await GetSetAsync<T>(connection, classMap, wherePredicate, sort, firstResult, maxResults, transaction, commandTimeout, colsToSelect).ConfigureAwait(false); */
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.Count{T}"/>.
        /// </summary>
        public async Task<int> CountAsync<T>(IDbConnection connection, object predicate = null, IDbTransaction transaction = null,
            int? commandTimeout = null, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await Task.FromResult(Count<T>(connection, predicate, transaction, commandTimeout, includedProperties));

            /*GetMapAndPredicate<T>(predicate, out var classMap, out var wherePredicate);
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Count(classMap, wherePredicate, parameters, includedProperties);

            var dynamicParameters = GetDynamicParameters(parameters);
            LastExecutedCommand = sql;
            return (int)(await connection.QueryAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false)).Single().Total;*/
        }

        public async Task<IMultipleResultReader> GetMultipleAsync(IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction, 
            int? commandTimeout, IList<IReferenceMap> includedProperties = null)
        {
            if (SqlGenerator.SupportsMultipleStatements())
            {
                return await Task.FromResult(GetMultipleByBatch(connection, predicate, transaction, commandTimeout, includedProperties));
            }

            return await Task.FromResult(GetMultipleBySequence(connection, predicate, transaction, commandTimeout, includedProperties));
        }
        #endregion

        #region Private implementations

        private async Task<dynamic> InternalInsertAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout,
            IClassMapper classMap, IList<IMemberMap> nonIdentityKeyProperties, IMemberMap identityColumn,
            IMemberMap triggerIdentityColumn, IList<IMemberMap> sequenceIdentityColumn) where T : class
        {
            return await Task.FromResult(InternalInsert<T>(connection, entity, transaction, commandTimeout, classMap, nonIdentityKeyProperties, identityColumn, triggerIdentityColumn, sequenceIdentityColumn));

            /*DynamicParameters dynamicParameters = null;

            foreach (var column in nonIdentityKeyProperties)
            {
                if (column.KeyType == KeyType.Guid && (Guid)column.GetValue(entity) == Guid.Empty)
                {
                    var comb = SqlGenerator.Configuration.GetNextGuid();
                    column.SetValue(entity, comb);
                }
            }

            IDictionary<string, object> keyValues = new ExpandoObject();
            var sql = SqlGenerator.Insert(classMap);
            if (triggerIdentityColumn != null || identityColumn != null)
            {
                var keyColumn = triggerIdentityColumn ?? identityColumn;
                object keyValue = null;

                dynamicParameters = new DynamicParameters();
                foreach (var prop in entity.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.Name != keyColumn.Name))
                {
                    var propValue = prop.GetValue(entity, null);
                    var parameter = ReflectionHelper.GetParameter(typeof(T), SqlGenerator, prop.Name, propValue);
                    dynamicParameters.Add(GetSimpleAliasFromColumnAlias(prop.Name), parameter.Value, parameter.DbType,
                                          parameter.ParameterDirection, parameter.Size, parameter.Precision,
                                          parameter.Scale);
                }

                if (triggerIdentityColumn != null)
                {
                    // defaultValue need for identify type of parameter
                    var defaultValue = entity.GetType().GetProperty(triggerIdentityColumn.Name).GetValue(entity, null);
                    dynamicParameters.Add("IdOutParam", direction: ParameterDirection.Output, value: defaultValue);

                    LastExecutedCommand = sql;
                    connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);

                    keyValue = dynamicParameters.Get<object>(SqlGenerator.Configuration.Dialect.ParameterPrefix + "IdOutParam");
                }
                else
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
                        LastExecutedCommand = sql;
                        result = connection.Query<long>(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text);
                    }

                    // We are only interested in the first identity, but we are iterating over all resulting items (if any).
                    // This makes sure that ADO.NET drivers (like MySql) won't actively terminate the query.
                    var hasResult = false;
                    foreach (var identityValue in result)
                    {
                        if (hasResult)
                        {
                            continue;
                        }
                        keyValue = identityValue;
                        hasResult = true;
                    }

                    if (!hasResult)
                    {
                        throw new InvalidOperationException("The source sequence is empty.");
                    }
                }

                keyValues.Add(keyColumn.Name, keyValue);
                keyColumn.SetValue(entity, keyValue);
            }
            else
            {
                dynamicParameters = GetDynamicParameters(classMap, entity, true);

                if (sequenceIdentityColumn.Count > 0)
                {
                    if (sequenceIdentityColumn.Count > 1)
                        throw new ArgumentException("SequenceIdentity generator cannot be used with multi-column keys");

                    var seqIdentityColumn = sequenceIdentityColumn[0];

                    var query = $"select {seqIdentityColumn.SequenceName}.nextval seq from dual";
                    var value = connection.ExecuteScalar<int>(query);

                    keyValues.Add(seqIdentityColumn.Name, value);
                    seqIdentityColumn.SetValue(entity, value);

                    var parameter = ReflectionHelper.GetParameter(typeof(T), SqlGenerator, seqIdentityColumn.Name, seqIdentityColumn.GetValue(entity));
                    dynamicParameters.Add(GetSimpleAliasFromColumnAlias(parameter.Name), parameter.Value, parameter.DbType,
                                      parameter.ParameterDirection, parameter.Size, parameter.Precision,
                                      parameter.Scale);
                }
                else if (nonIdentityKeyProperties != null)
                {
                    foreach (var prop in nonIdentityKeyProperties)
                    {
                        var parameter = ReflectionHelper.GetParameter(typeof(T), SqlGenerator, prop.Name, prop.GetValue(entity));
                        dynamicParameters.Add(GetSimpleAliasFromColumnAlias(parameter.Name), parameter.Value, parameter.DbType,
                                      parameter.ParameterDirection, parameter.Size, parameter.Precision,
                                      parameter.Scale);
                    }
                }

                LastExecutedCommand = sql;
                connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text);
            }

            foreach (var column in nonIdentityKeyProperties)
            {
                keyValues.Add(column.Name, column.GetValue(entity));
            }

            if (keyValues.Count == 1)
            {
                return Task.FromResult(keyValues.First().Value);
            }

            return await Task.FromResult(keyValues as dynamic); */
        }

        private async Task<bool> InternalUpdateAsync<T>(IDbConnection connection, T entity, IClassMapper classMap, IPredicate predicate, IDbTransaction transaction, 
            IList<IProjection> cols, int? commandTimeout, bool ignoreAllKeyProperties = false) where T : class
        {
            return await Task.FromResult(InternalUpdate<T>(connection, entity, classMap, predicate, transaction, cols, commandTimeout, ignoreAllKeyProperties));

            /*var parameters = new Dictionary<string, object>();
            string sql = SqlGenerator.Update(classMap, predicate, parameters, ignoreAllKeyProperties, cols);

            var dynamicParameters = GetDynamicParameters(classMap, entity, true);
            dynamicParameters.AddDynamicParams(GetDynamicParameters(parameters));

            LastExecutedCommand = sql;
            return await Task.FromResult(connection.ExecuteAsync(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).Result > 0); */
        }

        private async Task<bool> InternalUpdateAsync<T>(IDbConnection connection, T entity, IDbTransaction transaction, IList<IProjection> cols, 
            int? commandTimeout, bool ignoreAllKeyProperties = false) where T : class
        {
            GetMapAndPredicate<T>(entity, out var classMap, out var predicate, true);
            return await InternalUpdateAsync(connection, entity, classMap, predicate, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        }

        private async void InternalUpdateAsync<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, IList<IProjection> cols, 
            int? commandTimeout, bool ignoreAllKeyProperties = false) where T : class
        {
            GetMapAndPredicate<T>(entities.FirstOrDefault(), out var classMap, out var predicate, true);

            foreach (var e in entities)
                await InternalUpdateAsync(connection, e, classMap, predicate, transaction, cols, commandTimeout, ignoreAllKeyProperties);
        }

        private async Task<T> InternalGetAsync<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await Task.FromResult(InternalGetListAutoMap<T>(connection, id, null, transaction, commandTimeout, true, colsToSelect, includedProperties));
        }

        private async Task<IEnumerable<T>> InternalGetListAutoMapAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, IDbTransaction transaction, 
            int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await Task.FromResult(InternalGetListAutoMap<T>(connection, predicate, sort, transaction, commandTimeout, buffered, colsToSelect, includedProperties));
        }

        private async Task<IEnumerable<T>> InternalGetPageAutoMapAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, 
            IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await Task.FromResult(InternalGetPageAutoMap<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect, includedProperties));
        }

        private async Task<IEnumerable<T>> InternalGetSetAsync<T>(IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, 
            IDbTransaction transaction, int? commandTimeout, bool buffered, IList<IProjection> colsToSelect, IList<IReferenceMap> includedProperties = null) where T : class
        {
            return await Task.FromResult(InternalGetSet<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, colsToSelect, includedProperties));
        }
        #endregion

        #region Helpers

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetList{T}"/>.
        /// </summary>
        protected async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect = null) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.Select(classMap, predicate, sort, parameters, colsToSelect);
            var dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false);
        }

        protected async Task<IEnumerable<T>> GetListAutoMapAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect = null) where T : class
        {
            var query = await GetListAsync<dynamic>(connection, classMap, predicate, sort, transaction, commandTimeout, colsToSelect);
            var data = query.ToList();

            return await Task.FromResult(AutoMapper.MapDynamic<T>(data, false)).ConfigureAwait(false);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
        /// </summary>
        protected async Task<IEnumerable<T>> GetPageAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect = null) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.SelectPaged(classMap, predicate, sort, page, resultsPerPage, parameters, colsToSelect);
            var dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetPage{T}"/>.
        /// </summary>
        protected async Task<IEnumerable<T>> GetPageAutoMapAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect = null) where T : class
        {
            var query = await GetPageAsync<dynamic>(connection, classMap, predicate, sort, page, resultsPerPage, transaction, commandTimeout, colsToSelect);
            var data = query.ToList();

            return await Task.FromResult(AutoMapper.MapDynamic<T>(data, false)).ConfigureAwait(false);
        }

        /// <summary>
        /// The asynchronous counterpart to <see cref="IDapperImplementor.GetSet{T}"/>.
        /// </summary>
        protected async Task<IEnumerable<T>> GetSetAsync<T>(IDbConnection connection, IClassMapper classMap, IPredicate predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, IList<IProjection> colsToSelect = null) where T : class
        {
            var parameters = new Dictionary<string, object>();
            var sql = SqlGenerator.SelectSet(classMap, predicate, sort, firstResult, maxResults, parameters, colsToSelect);
            var dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return await connection.QueryAsync<T>(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text).ConfigureAwait(false);
        }
        #endregion
    }
}
