using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dapper.Extensions.Linq.Core.Builder;

namespace Dapper.Extensions.Linq.Core.Repositories
{
    public interface IRepository<T> where T : class, IEntity
    {
        T Get(int id);
        int Insert(T item);
        bool Update(T item);
        bool Delete(T item);
        IList<T> GetList();
        IEntityBuilder<T> Query(Expression<Func<T, bool>> predicate);
        IEnumerable<T> Query(string sql, object param = null);
        object QueryScalar(string sql, object param = null);
    }
}
