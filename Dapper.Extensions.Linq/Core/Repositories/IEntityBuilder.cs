using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Repositories
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
    }
}