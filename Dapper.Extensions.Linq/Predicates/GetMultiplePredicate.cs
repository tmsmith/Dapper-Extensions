using System.Collections.Generic;
using Dapper.Extensions.Linq.Core.Predicates;

namespace Dapper.Extensions.Linq.Predicates
{
    public class GetMultiplePredicate : IGetMultiplePredicate
    {
        private readonly List<GetMultiplePredicateItem> _items;

        public GetMultiplePredicate()
        {
            _items = new List<GetMultiplePredicateItem>();
        }

        public IEnumerable<IGetMultiplePredicateItem> Items => _items.AsReadOnly();

        public void Add<T>(IPredicate predicate, IList<ISort> sort = null) where T : class
        {
            _items.Add(new GetMultiplePredicateItem
                           {
                               Value = predicate,
                               Type = typeof(T),
                               Sort = sort
                           });
        }

        public void Add<T>(object id) where T : class
        {
            _items.Add(new GetMultiplePredicateItem
                           {
                               Value = id,
                               Type = typeof (T)
                           });
        }
    }
}