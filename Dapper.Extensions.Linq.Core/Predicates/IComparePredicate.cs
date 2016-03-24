using Dapper.Extensions.Linq.Core.Enums;

namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IComparePredicate : IBasePredicate
    {
        Operator Operator { get; set; }
        bool Not { get; set; }
    }
}