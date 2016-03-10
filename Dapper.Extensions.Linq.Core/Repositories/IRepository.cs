using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dapper.Extensions.Linq.Core.Builder;

namespace Dapper.Extensions.Linq.Core.Repositories
{
    public interface IRepository<T> where T : class, IEntity
    {
        T Get(Guid id);
        T Get(int id);
        dynamic Insert(T item);
        bool Update(T item);
        bool Delete(T item);
        IList<T> GetList();
        IEntityBuilder<T> Query(Expression<Func<T, bool>> predicate = null);
        int Count(Expression<Func<T, bool>> predicate = null);
        bool Delete(Expression<Func<T, bool>> predicate = null);
        IEnumerable<T> Query(string sql, object param = null, int? timeout = null);
        IEnumerable<dynamic> QueryDynamic(string sql, object param = null, int? timeout = null);
        object QueryScalar(string sql, object param = null, int? timeout = null);
    }
}
