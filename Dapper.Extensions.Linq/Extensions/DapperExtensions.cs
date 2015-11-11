using System;
using System.Collections.Generic;
using System.Data;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Core.Implementor;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Implementor;
using Dapper.Extensions.Linq.Sql;

namespace Dapper.Extensions.Linq.Extensions
{
    public static class DapperExtensions
    {
        private readonly static object Lock = new object();


        private static IDapperImplementor _instance;

        private static IDapperImplementor Instance
        {
            get
            {
                if (DapperConfiguration.Instance == null)
                    throw new NullReferenceException("DapperConfiguration has not been built.");


                if (_instance != null && _instance.SqlGenerator.Configuration == DapperConfiguration.Instance)
                    return _instance;

                lock (Lock)
                {
                    var sqlGeneratorImpl = new SqlGeneratorImpl(DapperConfiguration.Instance);
                    _instance = new DapperImplementor(sqlGeneratorImpl);
                }

                return _instance;
            }
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
        public static bool Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Update(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a delete query for the specified entity.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Delete(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Excutes a delete query based on a predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="predicate"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static bool Delete<T>(this IDbConnection connection, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Delete<T>(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// </summary>
        public static IEnumerable<T> GetList<T>(this IDbConnection connection, object predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false, int? topRecords = null) where T : class
        {
            return Instance.GetList<T>(connection, predicate, sort, transaction, commandTimeout, buffered, topRecords);
        }

        /// <summary>
        /// Executes a count query using the specified predicate if supplied
        /// </summary>
        public static int Count<T>(this IDbConnection connection, object predicate = null, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Count<T>(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        /// Gets the appropriate mapper for the specified type T. 
        /// If the mapper for the type is not yet created, a new mapper is generated from the mapper type specifed by DefaultMapper.
        /// </summary>
        public static IClassMapper GetMap<T>() where T : class
        {
            return Instance.SqlGenerator.Configuration.GetMap<T>();
        }

        /// <summary>
        /// Generates a COMB Guid which solves the fragmented index issue.
        /// See: http://davybrion.com/blog/2009/05/using-the-guidcomb-identifier-strategy
        /// </summary>
        public static Guid GetNextGuid()
        {
            return Instance.SqlGenerator.Configuration.GetNextGuid();
        }
    }
}
