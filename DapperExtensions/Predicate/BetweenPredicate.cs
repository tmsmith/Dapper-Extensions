using DapperExtensions.Sql;
using System.Collections.Generic;

namespace DapperExtensions.Predicate
{
    public struct BetweenValues
    {
        public object Value1 { get; set; }
        public object Value2 { get; set; }
    }

    public interface IBetweenPredicate : IPredicate
    {
        string PropertyName { get; set; }
        BetweenValues Value { get; set; }
        bool Not { get; set; }
    }

    public class BetweenPredicate<T> : BasePredicate, IBetweenPredicate
    {
        public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            var columnName = GetColumnName(typeof(T), sqlGenerator, PropertyName);
            var parameter1 = ReflectionHelper.GetParameter(typeof(T), sqlGenerator, PropertyName, Value.Value1);
            var parameter2 = ReflectionHelper.GetParameter(typeof(T), sqlGenerator, PropertyName, Value.Value2);
            var propertyName1 = parameters.SetParameterName(parameter1, sqlGenerator.Configuration.Dialect.ParameterPrefix);
            var propertyName2 = parameters.SetParameterName(parameter2, sqlGenerator.Configuration.Dialect.ParameterPrefix);
            return string.Format("({0} {1}BETWEEN {2} AND {3})", columnName, Not ? "NOT " : string.Empty, propertyName1, propertyName2);
        }

        public BetweenValues Value { get; set; }

        public bool Not { get; set; }
    }
}
