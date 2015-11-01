using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IGetMultiplePredicate
    {
        IEnumerable<IGetMultiplePredicateItem> Items { get; }
        void Add<T>(IPredicate predicate, IList<ISort> sort = null) where T : class;
        void Add<T>(object id) where T : class;
    }
}