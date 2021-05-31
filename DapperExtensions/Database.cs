using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DapperExtensions
{
    #region Interfaces
    public interface IBaseDatabase : IDisposable
    {
        bool HasActiveTransaction { get; }
        IDbConnection Connection { get; }
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void Commit();
        void Rollback();
        void RunInTransaction(Action action);
        T RunInTransaction<T>(Func<T> func);
    }

    public interface IDatabase : IBaseDatabase
    {
        T Get<T>(dynamic id, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        T Get<T>(dynamic id, int? commandTimeout = null) where T : class;
        void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        void Insert<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class;
        dynamic Insert<T>(T entity, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        dynamic Insert<T>(T entity, int? commandTimeout = null) where T : class;
        bool Update<T>(T entity, IDbTransaction transaction, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;
        bool Update<T>(T entity, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;
        bool Delete<T>(T entity, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        bool Delete<T>(T entity, int? commandTimeout = null) where T : class;
        bool Delete<T>(object predicate, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        bool Delete<T>(object predicate, int? commandTimeout = null) where T : class;
        IEnumerable<T> GetList<T>(object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetList<T>(object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, int? commandTimeout = null, bool buffered = true) where T : class;
        IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
        IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, int? commandTimeout, bool buffered) where T : class;
        int Count<T>(object predicate, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        int Count<T>(object predicate, int? commandTimeout = null) where T : class;
        IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout = null);
        IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, int? commandTimeout = null);
        Guid GetNextGuid();
        IClassMapper GetMap<T>() where T : class;
        void ClearCache();

    }

    public interface IAsyncDatabase : IBaseDatabase
    {
        Task<T> Get<T>(dynamic id, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        Task<T> Get<T>(dynamic id, int? commandTimeout = null) where T : class;
        void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        void Insert<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class;
        Task<dynamic> Insert<T>(T entity, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        Task<dynamic> Insert<T>(T entity, int? commandTimeout = null) where T : class;
        Task<bool> Update<T>(T entity, IDbTransaction transaction, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;
        Task<bool> Update<T>(T entity, int? commandTimeout = null, bool ignoreAllKeyProperties = false) where T : class;
        Task<bool> Delete<T>(T entity, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        Task<bool> Delete<T>(T entity, int? commandTimeout = null) where T : class;
        Task<bool> Delete<T>(object predicate, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        Task<bool> Delete<T>(object predicate, int? commandTimeout = null) where T : class;
        Task<IEnumerable<T>> GetList<T>(object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout = null, bool buffered = true) where T : class;
        Task<IEnumerable<T>> GetList<T>(object predicate = null, IList<ISort> sort = null, int? commandTimeout = null, bool buffered = true) where T : class;
        Task<IEnumerable<T>> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout = null, bool buffered = true) where T : class;
        Task<IEnumerable<T>> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, int? commandTimeout = null, bool buffered = true) where T : class;
        Task<IEnumerable<T>> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
        Task<IEnumerable<T>> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, int? commandTimeout, bool buffered) where T : class;
        Task<int> Count<T>(object predicate, IDbTransaction transaction, int? commandTimeout = null) where T : class;
        Task<int> Count<T>(object predicate, int? commandTimeout = null) where T : class;
        Task<IMultipleResultReader> GetMultiple(GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout = null);
        Task<IMultipleResultReader> GetMultiple(GetMultiplePredicate predicate, int? commandTimeout = null);
        Task<Guid> GetNextGuid();
        Task<IClassMapper> GetMap<T>() where T : class;
        void ClearCache();

    }
    #endregion

    #region Implementation
    public abstract class BaseDatabase : IBaseDatabase
    {
        protected IDbTransaction _transaction;

        protected BaseDatabase(IDbConnection connection)
        {
            Connection = connection;

            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        public bool HasActiveTransaction
        {
            get
            {
                return _transaction != null;
            }
        }

        public IDbConnection Connection { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Connection.State != ConnectionState.Closed)
                {
                    if (_transaction != null)
                    {
                        _transaction.Rollback();
                    }

                    Connection.Close();
                }
            }
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _transaction = Connection.BeginTransaction(isolationLevel);
        }

        public void Commit()
        {
            _transaction.Commit();
            _transaction = null;
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction = null;
        }

        public void RunInTransaction(Action action)
        {
            BeginTransaction();
            try
            {
                action?.Invoke();
                Commit();
            }
            catch (Exception)
            {
                if (HasActiveTransaction)
                {
                    Rollback();
                }

                throw;
            }
        }

        public T RunInTransaction<T>(Func<T> func)
        {
            BeginTransaction();
            try
            {
                var result = func.Invoke();
                Commit();
                return result;
            }
            catch (Exception)
            {
                if (HasActiveTransaction)
                {
                    Rollback();
                }

                throw;
            }
        }

        protected virtual void ClearCache(IDapperImplementor dapper)
        {
            dapper.SqlGenerator.Configuration.ClearCache();
        }
    }

    public class Database : BaseDatabase, IDatabase
    {
        private readonly IDapperImplementor _dapper;

        public Database(IDbConnection connection, ISqlGenerator sqlGenerator) : base(connection)
        {
            _dapper = new DapperImplementor(sqlGenerator);
        }

        public virtual T Get<T>(dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return (T)_dapper.Get<T>(Connection, id, transaction, commandTimeout);
        }

        public virtual T Get<T>(dynamic id, int? commandTimeout) where T : class
        {
            return (T)_dapper.Get<T>(Connection, id, _transaction, commandTimeout);
        }

        public virtual void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            _dapper.Insert(Connection, entities, transaction, commandTimeout);
        }

        public virtual void Insert<T>(IEnumerable<T> entities, int? commandTimeout) where T : class
        {
            _dapper.Insert(Connection, entities, _transaction, commandTimeout);
        }

        public virtual dynamic Insert<T>(T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return _dapper.Insert(Connection, entity, transaction, commandTimeout);
        }

        public virtual dynamic Insert<T>(T entity, int? commandTimeout) where T : class
        {
            return _dapper.Insert(Connection, entity, _transaction, commandTimeout);
        }

        public virtual bool Update<T>(T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties) where T : class
        {
            return _dapper.Update(Connection, entity, transaction, commandTimeout, ignoreAllKeyProperties);
        }

        public virtual bool Update<T>(T entity, int? commandTimeout, bool ignoreAllKeyProperties) where T : class
        {
            return _dapper.Update(Connection, entity, _transaction, commandTimeout, ignoreAllKeyProperties);
        }

        public virtual bool Delete<T>(T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return _dapper.Delete(Connection, entity, transaction, commandTimeout);
        }

        public virtual bool Delete<T>(T entity, int? commandTimeout) where T : class
        {
            return _dapper.Delete(Connection, entity, _transaction, commandTimeout);
        }

        public virtual bool Delete<T>(object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return _dapper.Delete<T>(Connection, predicate, transaction, commandTimeout);
        }

        public virtual bool Delete<T>(object predicate, int? commandTimeout) where T : class
        {
            return _dapper.Delete<T>(Connection, predicate, _transaction, commandTimeout);
        }

        public virtual IEnumerable<T> GetList<T>(object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetList<T>(Connection, predicate, sort, transaction, commandTimeout, buffered);
        }

        public virtual IEnumerable<T> GetList<T>(object predicate, IList<ISort> sort, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetList<T>(Connection, predicate, sort, _transaction, commandTimeout, buffered);
        }

        public virtual IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetPage<T>(Connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        public virtual IEnumerable<T> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetPage<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, buffered);
        }

        public virtual IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
        }

        public virtual IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, _transaction, commandTimeout, buffered);
        }

        public virtual int Count<T>(object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return _dapper.Count<T>(Connection, predicate, transaction, commandTimeout);
        }

        public virtual int Count<T>(object predicate, int? commandTimeout) where T : class
        {
            return _dapper.Count<T>(Connection, predicate, _transaction, commandTimeout);
        }

        public virtual IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
        {
            return _dapper.GetMultiple(Connection, predicate, transaction, commandTimeout);
        }

        public virtual IMultipleResultReader GetMultiple(GetMultiplePredicate predicate, int? commandTimeout)
        {
            return _dapper.GetMultiple(Connection, predicate, _transaction, commandTimeout);
        }

        public virtual void ClearCache()
        {
            ClearCache(_dapper);
        }

        public virtual Guid GetNextGuid()
        {
            return _dapper.SqlGenerator.Configuration.GetNextGuid();
        }

        public virtual IClassMapper GetMap<T>() where T : class
        {
            return _dapper.SqlGenerator.Configuration.GetMap<T>();
        }
    }

    public class AsyncDatabase : BaseDatabase, IAsyncDatabase
    {
        private readonly IDapperAsyncImplementor _dapper;

        public AsyncDatabase(IDbConnection connection, ISqlGenerator sqlGenerator) : base(connection)
        {
            _dapper = new DapperAsyncImplementor(sqlGenerator);
        }

        public async virtual Task<T> Get<T>(dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return await _dapper.GetAsync<T>(Connection, id, transaction, commandTimeout);
        }

        public async virtual Task<T> Get<T>(dynamic id, int? commandTimeout) where T : class
        {
            return await _dapper.GetAsync<T>(Connection, id, _transaction, commandTimeout);
        }

        public async virtual void Insert<T>(IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            await _dapper.InsertAsync(Connection, entities, transaction, commandTimeout);
        }

        public async virtual void Insert<T>(IEnumerable<T> entities, int? commandTimeout) where T : class
        {
            await _dapper.InsertAsync(Connection, entities, _transaction, commandTimeout);
        }

        public async virtual Task<dynamic> Insert<T>(T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return await _dapper.InsertAsync(Connection, entity, transaction, commandTimeout);
        }

        public async virtual Task<dynamic> Insert<T>(T entity, int? commandTimeout) where T : class
        {
            return await _dapper.InsertAsync(Connection, entity, _transaction, commandTimeout);
        }

        public async virtual Task<bool> Update<T>(T entity, IDbTransaction transaction, int? commandTimeout, bool ignoreAllKeyProperties) where T : class
        {
            return await _dapper.UpdateAsync(Connection, entity, transaction, commandTimeout, ignoreAllKeyProperties);
        }

        public async virtual Task<bool> Update<T>(T entity, int? commandTimeout, bool ignoreAllKeyProperties) where T : class
        {
            return await _dapper.UpdateAsync(Connection, entity, _transaction, commandTimeout, ignoreAllKeyProperties);
        }

        public async virtual Task<bool> Delete<T>(T entity, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return await _dapper.DeleteAsync(Connection, entity, transaction, commandTimeout);
        }

        public async virtual Task<bool> Delete<T>(T entity, int? commandTimeout) where T : class
        {
            return await _dapper.DeleteAsync(Connection, entity, _transaction, commandTimeout);
        }

        public async virtual Task<bool> Delete<T>(object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return await _dapper.DeleteAsync<T>(Connection, predicate, transaction, commandTimeout);
        }

        public async virtual Task<bool> Delete<T>(object predicate, int? commandTimeout) where T : class
        {
            return await _dapper.DeleteAsync<T>(Connection, predicate, _transaction, commandTimeout);
        }

        public async virtual Task<IEnumerable<T>> GetList<T>(object predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            return await _dapper.GetListAsync<T>(Connection, predicate, sort, transaction, commandTimeout, buffered);
        }

        public async virtual Task<IEnumerable<T>> GetList<T>(object predicate, IList<ISort> sort, int? commandTimeout, bool buffered) where T : class
        {
            return await _dapper.GetListAsync<T>(Connection, predicate, sort, _transaction, commandTimeout, buffered);
        }

        public async virtual Task<IEnumerable<T>> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            return await _dapper.GetPageAsync<T>(Connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        public async virtual Task<IEnumerable<T>> GetPage<T>(object predicate, IList<ISort> sort, int page, int resultsPerPage, int? commandTimeout, bool buffered) where T : class
        {
            return await _dapper.GetPageAsync<T>(Connection, predicate, sort, page, resultsPerPage, _transaction, commandTimeout, buffered);
        }

        public async virtual Task<IEnumerable<T>> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            return await _dapper.GetSetAsync<T>(Connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
        }

        public async virtual Task<IEnumerable<T>> GetSet<T>(object predicate, IList<ISort> sort, int firstResult, int maxResults, int? commandTimeout, bool buffered) where T : class
        {
            return await _dapper.GetSetAsync<T>(Connection, predicate, sort, firstResult, maxResults, _transaction, commandTimeout, buffered);
        }

        public async virtual Task<int> Count<T>(object predicate, IDbTransaction transaction, int? commandTimeout) where T : class
        {
            return await _dapper.CountAsync<T>(Connection, predicate, transaction, commandTimeout);
        }

        public async virtual Task<int> Count<T>(object predicate, int? commandTimeout) where T : class
        {
            return await _dapper.CountAsync<T>(Connection, predicate, _transaction, commandTimeout);
        }

        public async virtual Task<IMultipleResultReader> GetMultiple(GetMultiplePredicate predicate, IDbTransaction transaction, int? commandTimeout)
        {
            return await _dapper.GetMultipleAsync(Connection, predicate, transaction, commandTimeout);
        }

        public async virtual Task<IMultipleResultReader> GetMultiple(GetMultiplePredicate predicate, int? commandTimeout)
        {
            return await _dapper.GetMultipleAsync(Connection, predicate, _transaction, commandTimeout);
        }

        public async virtual Task<Guid> GetNextGuid()
        {
            return await Task.FromResult(_dapper.SqlGenerator.Configuration.GetNextGuid());
        }

        public async virtual Task<IClassMapper> GetMap<T>() where T : class
        {
            return await Task.FromResult(_dapper.SqlGenerator.Configuration.GetMap<T>());
        }

        public virtual void ClearCache()
        {
            ClearCache(_dapper);
        }
    }
    #endregion
}