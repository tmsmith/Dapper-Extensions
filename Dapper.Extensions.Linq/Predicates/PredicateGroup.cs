using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Core.Sql;
using Dapper.Extensions.Linq.Sql;

namespace Dapper.Extensions.Linq.Predicates
{
    /// <summary>
    /// Groups IPredicates together using the specified group operator.
    /// </summary>
    public class PredicateGroup : IPredicateGroup
    {
        public GroupOperator Operator { get; set; }
        public IList<IPredicate> Predicates { get; set; }
        public string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
        {
            string seperator = Operator == GroupOperator.And ? " AND " : " OR ";
            return "(" + Predicates.Aggregate(new StringBuilder(),
                (sb, p) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(p.GetSql(sqlGenerator, parameters)),
                sb =>
                {
                    var s = sb.ToString();
                    if (s.Length == 0) return sqlGenerator.Configuration.Dialect.EmptyExpression; 
                    return s;
                }
                ) + ")";
        }
    }
}