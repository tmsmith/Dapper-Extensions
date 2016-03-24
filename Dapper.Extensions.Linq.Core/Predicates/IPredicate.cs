using System.Collections.Generic;
using Dapper.Extensions.Linq.Core.Sql;

namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IPredicate
    {
        string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters);
    }
}