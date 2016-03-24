using Dapper.Extensions.Linq.Core.Predicates;

namespace Dapper.Extensions.Linq.Predicates
{
    public interface IBetweenPredicate : IPredicate
    {
        string PropertyName { get; set; }
        BetweenValues Value { get; set; }
        bool Not { get; set; }

    }
}