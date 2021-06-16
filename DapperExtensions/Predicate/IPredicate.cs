using DapperExtensions.Sql;
using System.Collections.Generic;

namespace DapperExtensions.Predicate
{
    public interface IPredicate
    {
        string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false);
    }
}
