using System;
using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IGetMultiplePredicateItem
    {
        object Value { get; set; }
        Type Type { get; set; }
        IList<ISort> Sort { get; set; }
    }
}