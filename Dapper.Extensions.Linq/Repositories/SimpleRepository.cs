using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dapper.Extensions.Linq.Builder;
using Dapper.Extensions.Linq.Core;
using Dapper.Extensions.Linq.Core.Builder;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Core.Sessions;

namespace Dapper.Extensions.Linq.Repositories
{
    public class SimpleRepository<T> : IRepository<T> where T : class, IEntity
    {
        protected IDapperSessionContext SessionContext { get; }

        public SimpleRepository(IDapperSessionContext sessionContext)
        {
            SessionContext = sessionContext;
        }

        public virtual T Get(int id)
        {
            return GetCurrentSession().Get<T>(id, GetCurrentSession().Transaction);
        }

        public virtual int Insert(T item)
        {
            return GetCurrentSession().Insert(item, GetCurrentSession().Transaction);
        }

        public virtual bool Update(T item) { return GetCurrentSession().Update(item); }

        public virtual bool Delete(T item)
        {
            return GetCurrentSession().Delete(item, GetCurrentSession().Transaction);
        }

        public virtual IList<T> GetList()
        {
            return GetCurrentSession().GetList<T>(GetCurrentSession().Transaction).ToList();
        }

        public virtual IEntityBuilder<T> Query(Expression<Func<T, bool>> predicate)
        {
            return new EntityBuilder<T>(() =>
            {
                var session = GetCurrentSession();
                return session.GetList<T>(QueryBuilder<T>.FromExpression(predicate), null, session.Transaction);
            });
        }

        public virtual IEnumerable<T> Query(string sql, object param = null)
        {
            return GetCurrentSession().Query<T>(sql, param, GetCurrentSession().Transaction);
        }

        public virtual IEnumerable<dynamic> DynamicQuery(string sql, object param = null)
        {
            return GetCurrentSession().Query<dynamic>(sql, param, GetCurrentSession().Transaction);
        }

        protected virtual IDapperSession GetCurrentSession() { return SessionContext.GetSession<T>(); }

        public object QueryScalar(string sql, object param = null)
        {
            return GetCurrentSession().ExecuteScalar(sql, param, GetCurrentSession().Transaction);
        }
    }
}
