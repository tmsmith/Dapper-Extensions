using System.Collections.Generic;
using Dapper.Extensions.Linq.Core.Enums;

namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IPredicateGroup : IPredicate
    {
        GroupOperator Operator { get; set; }
        IList<IPredicate> Predicates { get; set; }
    }
}