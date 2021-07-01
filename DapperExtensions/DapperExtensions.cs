using DapperExtensions.Mapper;
using DapperExtensions.Predicate;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperExtensions
{
    public static class DapperExtensions
    {
        private readonly static object _lock = new object();
        private static Func<IDapperExtensionsConfiguration, IDapperImplementor> _instanceFactory;
        private static IDapperImplementor _instance;
        private static IDapperExtensionsConfiguration _configuration;

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
                Configure(value, _configuration.MappingAssemblies, SqlDialect);
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
                Configure(DefaultMapper, _configuration.MappingAssemblies, value);
            }
        }

        /// <summary>
        /// Get or sets the Dapper Extensions Implementation Factory.
        /// </summary>
        public static Func<IDapperExtensionsConfiguration, IDapperImplementor> InstanceFactory
        {
            get
            {
                return _instanceFactory ??= config => new DapperImplementor(new SqlGeneratorImpl(config));
            }
            set
            {
                _instanceFactory = value;
                Configure(_configuration.DefaultMapper, _configuration.MappingAssemblies, _configuration.Dialect);
            }
        }

        public static SqlInjection GetOrSetSqlInjection(this Type entityType, SqlInjection sqlInjection = null)
        {
            return _configuration.GetOrSetSqlInjection(entityType, sqlInjection);
        }

        /// <summary>
        /// Gets the Dapper Extensions Implementation
        /// </summary>
        private static IDapperImplementor Instance
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
        /// Configure DapperExtensions extension methods.
        /// </summary>
        /// <param name="defaultMapper"></param>
        /// <param name="mappingAssemblies"></param>
        /// <param name="sqlDialect"></param>
        public static void Configure(this IDapperExtensionsConfiguration configuration)
        {
            _instance = null;
            _configuration = configuration;
        }

        static DapperExtensions()
        {
            Configure(typeof(AutoClassMapper<>), new List<Assembly>(), new SqlServerDialect());
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
        /// <param name="defaultMapper"></param>
        /// <param name="mappingAssemblies"></param>
        /// <param name="sqlDialect"></param>
        public static void Configure(this Type defaultMapper, IList<Assembly> mappingAssemblies, ISqlDialect sqlDialect)
        {
            Configure(new DapperExtensionsConfiguration(defaultMapper, mappingAssemblies, sqlDialect));
        }

        /// <summary>
        /// Executes a query for the specified id, returning the data typed as per T
        /// </summary>
        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return (T)Instance.Get<T>(connection, id, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a query for the specified id, returning the data typed as per T
        /// </summary>
        public static TOut GetPartial<TIn, TOut>(this IDbConnection connection, dynamic id, Expression<Func<TIn, TOut>> func, IDbTransaction transaction = null, int? commandTimeout = null) where TIn : class where TOut : class
        {
            return Instance.GetPartial<TIn, TOut>(connection, func, id, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes an insert query for the specified entity.
        /// </summary>
        public static void Insert<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Instance.Insert(connection, entities, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes an insert query for the specified entity, returning the primary key.
        /// If the entity has a single key, just the value is returned.
        /// If the entity has a composite key, an IDictionary&lt;string, object&gt; is returned with the key values.
        /// The key value for the entity will also be updated if the KeyType is a Guid or Identity.
        /// </summary>
        public static dynamic Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Insert(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes an update query for the specified entity.
        /// </summary>
        public static bool Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
        {
            return Instance.Update(connection, entity, transaction, commandTimeout, ignoreAllKeyProperties);
        }

        /// <summary>
        /// Executes an update query for the specified entity.
        /// </summary>
        public static void Update<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class
        {
            Instance.Update(connection, entities, transaction, commandTimeout, ignoreAllKeyProperties);
        }

        /// <summary>
        /// Executes some column an update query for the specified entity, as typed by LINq expresion
        /// </summary>
        public static bool UpdatePartial<TIn, TOut>(this IDbConnection connection, TIn entity, Expression<Func<TIn, TOut>> func, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where TIn : class where TOut : class
        {
            return Instance.UpdatePartial(connection, entity, func, transaction, commandTimeout, ignoreAllKeyProperties);
        }

        /// <summary>
        /// Executes some column an update query for the specified entity, as typed by LINq expresion
        /// </summary>
        public static void UpdatePartial<TIn, TOut>(this IDbConnection connection, IEnumerable<TIn> entities, Expression<Func<TIn, TOut>> func, IDbTransaction transaction = null, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where TIn : class where TOut : class
        {
            Instance.UpdatePartial(connection, entities, func, transaction, commandTimeout, ignoreAllKeyProperties);
        }

        /// <summary>
        /// Executes a delete query for the specified entity.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Delete(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a delete query for the specified entity.
        /// </summary>
        public static void Delete<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Instance.Delete(connection, entities, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a delete query using the specified predicate.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Delete<T>(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// </summary>
        public static IEnumerable<T> GetList<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return Instance.GetList<T>(connection, predicate, sort, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per LINq Expression.
        /// </summary>
        public static IEnumerable<TOut> GetPartialList<TIn, TOut>(this IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where TIn : class
        {
            return Instance.GetPartialList(connection, func, predicate, sort, transaction, commandTimeout, buffered);

        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T with relacionated classes.
        /// </summary>
        public static IEnumerable<T> GetListAutoMap<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return Instance.GetListAutoMap<T>(connection, predicate, sort, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T with relacionated classes.
        /// </summary>
        public static IEnumerable<TOut> GetPartialListAutoMap<TIn, TOut>(this IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where TIn : class
        {
            return Instance.GetPartialListAutoMap(connection, func, predicate, sort, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified page and resultsPerPage.
        /// </summary>
        public static IEnumerable<T> GetPage<T>(this IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return Instance.GetPage<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per LINq expression.
        /// Data returned is dependent upon the specified page and resultsPerPage.
        /// </summary>
        public static IEnumerable<TOut> GetPartialPage<TIn, TOut>(this IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where TIn : class where TOut : class
        {
            return Instance.GetPartialPage(connection, func, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        public static IEnumerable<T> GetPageAutoMap<T>(this IDbConnection connection, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return Instance.GetPageAutoMap<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per LINq expression.
        /// Data returned is dependent upon the specified page and resultsPerPage.
        /// </summary>
        public static IEnumerable<TOut> GetPartialPageAutoMap<TIn, TOut>(this IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where TIn : class where TOut : class
        {
            return Instance.GetPartialPageAutoMap(connection, func, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified firstResult and maxResults.
        /// </summary>
        public static IEnumerable<T> GetSet<T>(this IDbConnection connection, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return Instance.GetSet<T>(connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per LINq expression.
        /// Data returned is dependent upon the specified firstResult and maxResults.
        /// </summary>
        public static IEnumerable<TOut> GetPartialSet<TIn, TOut>(this IDbConnection connection, Expression<Func<TIn, TOut>> func, object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where TIn : class where TOut : class
        {
            return Instance.GetPartialSet(connection, func, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a query using the specified predicate, returning an integer that represents the number of rows that match the query.
        /// </summary>
        public static int Count<T>(this IDbConnection connection, object predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Count<T>(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a select query for multiple objects, returning IMultipleResultReader for each predicate.
        /// </summary>
        public static IMultipleResultReader GetMultiple(this IDbConnection connection, GetMultiplePredicate predicate, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            return Instance.GetMultiple(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        /// Gets the appropriate mapper for the specified type T.
        /// If the mapper for the type is not yet created, a new mapper is generated from the mapper type specifed by DefaultMapper.
        /// </summary>
        public static IClassMapper GetMap<T>() where T : class
        {
            return _configuration.GetMap<T>();
        }

        public static IClassMapper GetMap(this Type entityType)
        {
            return _configuration.GetMap(entityType);
        }

        public static IList<Assembly> MappingAssemblies { get => _configuration.MappingAssemblies; }

        public static Type GetMapType(this Type entityType)
        {
            return Instance.SqlGenerator.Configuration.GetMapType(entityType);
        }

        /// <summary>
        /// Clears the ClassMappers for each type.
        /// </summary>
        public static void ClearCache()
        {
            Instance.SqlGenerator.Configuration.ClearCache();
        }

        /// <summary>
        /// Generates a COMB Guid which solves the fragmented index issue.
        /// See: http://davybrion.com/blog/2009/05/using-the-guidcomb-identifier-strategy
        /// </summary>
        public static Guid GetNextGuid()
        {
            return Instance.SqlGenerator.Configuration.GetNextGuid();
        }

        /// <summary>Gets columns from class mapped</summary>
        /// <param name="map">IClassMapper from class mapped</param>
        /// <returns>Array from ColumnName</returns>
        public static List<IColumn> AllMappedColumns { get; }

        public static IEnumerable<IMemberMap> GetIdentifiers(this IClassMapper map)
        {
            foreach (var item in map.Properties)
            {
                if (item.Name != item.Name
                    && (item.KeyType == KeyType.Identity ||
                        item.KeyType == KeyType.Assigned ||
                        item.KeyType == KeyType.SequenceIdentity)
                    )
                {
                    yield return item;
                }
            }

            foreach (var table in Instance.SqlGenerator.MappedTables)
            {
                var refs = DapperExtensions.GetMap(table.EntityType)
                    .Properties
                    .Where(c => c.ClassMapper.Identity == table.Identity && (c.KeyType == KeyType.Identity ||
                        c.KeyType == KeyType.Assigned ||
                        c.KeyType == KeyType.SequenceIdentity));

                foreach (var r in refs)
                {
                    yield return r;
                }
            }
        }

        /// <summary>
        /// Returns the name of the database table based on mappings done with POCO.
        /// Generic <T> is POCO class.
        /// See: https://stackoverflow.com/a/49321116/5779732
        /// </summary>
        public static string GetTableName<T>() where T : class
        {
            return new SqlGeneratorImpl(_configuration).GetTableName(GetMap<T>());
        }

        /// <summary>
        /// Returns the name of the database column based on mappings done with POCO.
        /// 'propertyName' is name of the property in POCO for which the column name should be returned.
        /// Generic <T> is POCO class.
        /// See: https://stackoverflow.com/a/49321116/5779732
        /// </summary>
        public static string GetColumnName<T>(string propertyName) where T : class
        {
            return new SqlGeneratorImpl(_configuration).GetColumnName(GetMap<T>(), propertyName, false);
        }

        /// <summary>
        /// Gets the last SQL command executed by the Dapper Extensions Implementation
        /// </summary>
        public static string LastExecutedCommand()
        {
            return Instance.LastExecutedCommand;
        }
    }
}
