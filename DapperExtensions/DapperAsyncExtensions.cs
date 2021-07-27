using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace DapperExtensions
{
    public static class DapperAsyncExtensions
    {
        private readonly static object _lock = new object();
        private static Func<IDapperExtensionsConfiguration, IDapperAsyncImplementor> _instanceFactory;
        private static IDapperAsyncImplementor _instance;
        private static IDapperExtensionsConfiguration _configuration;
        private static readonly Dictionary<Type, IList<IProjection>> ColsBuffer = new Dictionary<Type, IList<IProjection>>();

        /// <summary>
        /// Gets or sets the default class mapper to use when generating class maps. If not specified, AutoClassMapper<T> is used.
        /// DapperExtensions.Configure(Type, IList<Assembly>, ISqlDialect) can be used instead to set all values at once
        /// </summary>
        public static Type DefaultMapper
        {
            get
            {
                return _configuration.DefaultMapper;
            }

            set
            {
                Configure(value, _configuration.MappingAssemblies, _configuration.Dialect);
            }
        }

        /// <summary>
        /// Gets or sets the type of sql to be generated.
        /// DapperExtensions.Configure(Type, IList<Assembly>, ISqlDialect) can be used instead to set all values at once
        /// </summary>
        public static ISqlDialect SqlDialect
        {
            get
            {
                return _configuration.Dialect;
            }

            set
            {
                Configure(_configuration.DefaultMapper, _configuration.MappingAssemblies, value);
            }
        }

        /// <summary>
        /// Get or sets the Dapper Extensions Implementation Factory.
        /// </summary>
        public static Func<IDapperExtensionsConfiguration, IDapperAsyncImplementor> InstanceFactory
        {
            get
            {
                return _instanceFactory ??= config => new DapperAsyncImplementor(new SqlGeneratorImpl(_configuration));
            }
            set
            {
                _instanceFactory = value;
                Configure(_configuration.DefaultMapper, _configuration.MappingAssemblies, _configuration.Dialect);
            }
        }

        /// <summary>
        /// Gets the Dapper Extensions Implementation
        /// </summary>
        private static IDapperAsyncImplementor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = InstanceFactory(_configuration);
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Initializes the <see cref="DapperAsyncExtensions"/> class.
        /// </summary>
        static DapperAsyncExtensions()
        {
            Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new SqlServerDialect());
        }

        /// <summary>
        /// Return property liste from (anonymous) type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static IList<IProjection> GetBufferedCols<T>()
        {
            Type outType = typeof(T);

            lock (ColsBuffer)
            {
                if (ColsBuffer.TryGetValue(outType, out var cols) == false)
                {
                    cols = new List<IProjection>();

                    typeof(T).GetProperties().
                        Select(i => i.Name).
                        ToList().
                        ForEach(p => cols.Add(new Projection(p)));

                    ColsBuffer.Add(outType, cols);
                }

                return cols;
            }


        }

        /// <summary>
        /// Add other assemblies that Dapper Extensions will search if a mapping is not found in the same assembly of the POCO.
        /// </summary>
        /// <param name="assemblies"></param>
        public static void SetMappingAssemblies(this IList<Assembly> assemblies)
        {
            Configure(_configuration.DefaultMapper, assemblies, _configuration.Dialect);
        }

        /// <summary>
        /// Configure DapperExtensions extension methods.
        /// </summary>
        public static IDapperExtensionsConfiguration Configure(this IDapperExtensionsConfiguration configuration)
        {
            _instance = null;
            _configuration = configuration;
            return _configuration;
        }

        /// <summary>
        /// Configure DapperExtensions extension methods.
        /// </summary>
        /// <param name="defaultMapper"></param>
        /// <param name="mappingAssemblies"></param>
        /// <param name="sqlDialect"></param>
        public static IDapperExtensionsConfiguration Configure(this Type defaultMapper, IList<Assembly> mappingAssemblies, ISqlDialect sqlDialect)
        {
            return Configure(new DapperExtensionsConfiguration(defaultMapper, mappingAssemblies, sqlDialect));
        }

        /// <summary>
        /// Executes a query using the specified predicate, returning an integer that represents the number of rows that match the query.
        /// </summary>
        public static async Task<int> CountAsync<T>(this IDbConnection connection, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return await Instance.CountAsync<T>(connection, predicate, transaction, commandTimeout).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query for the specified id, returning the data typed as per T.
        /// </summary>
        public static async Task<T> GetAsync<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null,
            int? commandTimeout = null, bool buffered = false) where T : class
        {
            return await Instance.GetAsync<T>(connection, id, transaction, commandTimeout, buffered, null)).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query for the specified id, returning the data typed as per T.
        /// </summary>
        public static async Task<TOut> GetPartialAsync<TIn, TOut>(this IDbConnection connection, dynamic id, Expression<Func<TIn, TOut>> func,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where TIn : class where TOut : class
        {
            var cols = GetBufferedCols<TOut>();
            TIn obj = await Instance.GetAsync<TIn>(connection, id, transaction, commandTimeout, buffered, cols).ConfigureAwait(false);

            Func<TIn, TOut> f = func.Compile();
            return f.Invoke(obj);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// </summary>
        public static async Task<IEnumerable<T>> GetListAsync<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return await Instance.GetListAsync<T>(connection, predicate, sort, transaction, commandTimeout, buffered, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per Linq Expression.
        /// </summary>
        public static async Task<IEnumerable<TOut>> GetPartialListAsync<TIn, TOut>(this IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate = null,
            IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where TIn : class where TOut : class
        {
            var cols = GetBufferedCols<TOut>();
            List<TIn> list = (await Instance.GetListAsync<TIn>(connection, predicate, sort, transaction, commandTimeout, buffered, cols)).ToList();
            Func<TIn, TOut> f = func.Compile();
            return list.Select(i => f.Invoke(i));
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Contains Slapper.Automaper
        /// </summary>
        public static async Task<IEnumerable<T>> GetListAutoMapAsync<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null,
            IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null) where T : class
        {
            return await Instance.GetListAutoMapAsync<T>(connection, predicate, sort, transaction, commandTimeout, buffered, colsToSelect).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes an insert query for the specified entity.
        /// </summary>
        public static Task InsertAsync<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = default) where T : class
        {
            return Instance.InsertAsync(connection, entities, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes an insert query for the specified entity, returning the primary key.
        /// If the entity has a single key, just the value is returned.
        /// If the entity has a composite key, an IDictionary&lt;string, object&gt; is returned with the key values.
        /// The key value for the entity will also be updated if the KeyType is a Guid or Identity.
        /// </summary>
        public static Task<dynamic> InsertAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = default) where T : class
        {
            return Instance.InsertAsync(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes an update query for the specified entity.
        /// </summary>
        public static Task<bool> UpdateAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null,
            int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
        {
            return Instance.UpdateAsync(connection, entity, transaction, commandTimeout, ignoreAllKeyProperties, null);
        }

        /// <summary>
        /// Executes an update query for the specified entity.
        /// </summary>
        public static Task<bool> UpdatePartialAsync<TIn, TOut>(this IDbConnection connection, TIn entity, IDbTransaction transaction = null,
            int? commandTimeout = null, bool ignoreAllKeyProperties = false) where TIn : class where TOut : class
        {
            var cols = GetBufferedCols<TOut>();
            return Instance.UpdateAsync(connection, entity, transaction, commandTimeout, ignoreAllKeyProperties, cols);
        }

        /// <summary>
        /// Executes a delete query for the specified entity.
        /// </summary>
        public static Task<bool> DeleteAsync<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.DeleteAsync(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a delete query using the specified predicate.
        /// </summary>
        public static Task<bool> DeleteAsync<T>(this IDbConnection connection, object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.DeleteAsync(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified page and resultsPerPage.
        /// </summary>
        public static async Task<IEnumerable<T>> GetPageAsync<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1,
            int resultsPerPage = 10, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return await Instance.GetPageAsync<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per LINq expression.
        /// Data returned is dependent upon the specified page and resultsPerPage.
        /// </summary>
        public static async Task<IEnumerable<TOut>> GetPartialPageAsync<TIn, TOut>(this IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate = null,
            IList<ISort> sort = null, int page = 1, int resultsPerPage = 10, IDbTransaction transaction = null, int? commandTimeout = null,
            bool buffered = false) where TIn : class where TOut : class
        {
            var cols = GetBufferedCols<TOut>();

            List<TIn> list = (await Instance.GetPageAsync<TIn>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, null)).ToList();

            // Transform TIn object to Anonymous type
            Func<TIn, TOut> f = func.Compile();
            return list.Select(i => f.Invoke(i));

        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified page and resultsPerPage.
        /// Contains Slapper.Automaper
        /// </summary>
        public static async Task<IEnumerable<T>> GetPageAutoMapAsync<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, int page = 1,
            int resultsPerPage = 10, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, IList<IProjection> colsToSelect = null) where T : class
        {
            return await Instance.GetPageAutoMapAsync<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered, colsToSelect).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified firstResult and maxResults.
        /// </summary>
        public static async Task<IEnumerable<T>> GetSetAsync<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, int firstResult = 1,
            int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return await Instance.GetSetAsync<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified firstResult and maxResults.
        /// </summary>
        public static async Task<IEnumerable<TOut>> GetPartialSetAsync<TIn, TOut>(this IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate = null,
            IList<ISort> sort = null, int firstResult = 1, int maxResults = 10, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where TIn : class where TOut : class
        {
            var cols = GetBufferedCols<TOut>();

            List<TIn> list = (await Instance.GetSetAsync<TIn>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered, cols)).ToList();

            // Transform TIn object to Anonymous type
            Func<TIn, TOut> f = func.Compile();
            return list.Select(i => f.Invoke(i));
        }

        /// <summary>
        /// Gets the last SQL command executed by the Dapper Extensions Implementation
        /// </summary>
        public static async Task<string> LastExecutedCommandAsync()
        {
            return await Task.FromResult(Instance.LastExecutedCommand);
        }

        /// <summary>
        /// Gets the last SQL command executed by the Dapper Extensions Implementation
        /// </summary>
        public static void ClearCache()
        {
            Instance.SqlGenerator.Configuration.ClearCache();
        }

        /// <summary>
        /// Generates a COMB Guid which solves the fragmented index issue.
        /// See: http://davybrion.com/blog/2009/05/using-the-guidcomb-identifier-strategy
        /// </summary>
        public static async Task<Guid> GetNextGuid()
        {
            return await Task.FromResult(Instance.SqlGenerator.Configuration.GetNextGuid());
        }

        /// <summary>
        /// Gets the appropriate mapper for the specified type T.
        /// If the mapper for the type is not yet created, a new mapper is generated from the mapper type specifed by DefaultMapper.
        /// </summary>
        public static async Task<IClassMapper> GetMap<T>() where T : class
        {
            return await Task.FromResult(_configuration.GetMap<T>());
        }
    }
}
