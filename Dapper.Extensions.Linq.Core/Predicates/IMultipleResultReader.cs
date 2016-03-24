using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IMultipleResultReader
    {
        IEnumerable<T> Read<T>();
    }
}