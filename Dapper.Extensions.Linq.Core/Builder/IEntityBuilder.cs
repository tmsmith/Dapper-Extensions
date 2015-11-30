using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dapper.Extensions.Linq.Core.Builder
{
    public interface IEntityBuilder<T> where T : class, IEntity
    {
        bool Any();
        IEnumerable<T> AsEnumerable();
        IList<T> ToList();
        int Count();
        T Single();
        T SingleOrDefault();
        T FirstOrDefault();
        IEntityBuilder<T> OrderBy(Expression<Func<T, object>> expression);
        IEntityBuilder<T> OrderByDescending(Expression<Func<T, object>> expression);
    }
}