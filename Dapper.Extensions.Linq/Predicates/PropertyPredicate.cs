using System.Collections.Generic;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Core.Sql;
using Dapper.Extensions.Linq.Sql;

namespace Dapper.Extensions.Linq.Predicates
{
    public class PropertyPredicate<T, T2> : ComparePredicate, IPropertyPredicate
        where T : class
        where T2 : class
    {
        public string PropertyName2 { get; set; }

        public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters)
        {
            string columnName = GetColumnName(typeof(T), sqlGenerator, PropertyName);
            string columnName2 = GetColumnName(typeof(T2), sqlGenerator, PropertyName2);
            return string.Format("({0} {1} {2})", columnName, GetOperatorString(), columnName2);
        }
    }
}