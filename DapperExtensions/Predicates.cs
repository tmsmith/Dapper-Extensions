using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperExtensions
{
    public static class Predicates
    {
        public static IPredicate Field<T>(Expression<Func<T, object>> expression, Operator op, object value, bool not = false) where T : class
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return new FieldPredicate<T>
                       {
                           PropertyName = propertyInfo.Name,
                           Operator = op,
                           Value = value,
                           Not = not
                       };
        }
    }

    public interface IPredicate
    {
        string GetSql(IDictionary<string, object> parameters);
    }

    public interface IFieldPredicate : IPredicate
    {
        string PropertyName { get; set; }
        Operator Operator { get; set; }
        object Value { get; set; }
        bool Not { get; set; }
    }

    public class FieldPredicate<T> : IFieldPredicate where T : class
    {
        public string PropertyName { get; set; }
        public Operator Operator { get; set; }
        public object Value { get; set; }
        public bool Not { get; set; }

        public string GetSql(IDictionary<string, object> parameters)
        {
            IClassMapper map = DapperExtensions.GetMap<T>();
            IPropertyMap propertyMap = map.Properties.Single(p => p.Name == PropertyName);
            string columnName = DapperExtensions.Formatter.GetColumnName(map, propertyMap, false);
            if (Value == null)
            {
                return string.Format("({0} IS {1}NULL)", columnName, Not ? "NOT " : string.Empty);
            }

            string parameterName = string.Format("@{0}p{1}", PropertyName, parameters.Count);
            parameters.Add(parameterName, Value);
            return string.Format("({0} {1} {2})", columnName, GetOperatorString(), parameterName);
        }

        public string GetOperatorString()
        {
            switch (Operator)
            {
                case Operator.Gt:
                    return Not ? "<=" : ">";
                case Operator.Ge:
                    return Not ? "<" : ">=";
                case Operator.Lt:
                    return Not ? ">=" : "<";
                case Operator.Le:
                    return Not ? ">" : "<=";
                default:
                    return Not ? "<>" : "=";
            }
        }
    }

    public enum Operator
    {
        Eq,
        Gt,
        Ge,
        Lt,
        Le
    }

    //public interface IPredicateGroup : IPredicate
    //{
    //    GroupOperator Operator { get; set; }
    //    IList<IPredicate> Predicates { get; set; }
    //}

    //public class PredicateGroup : IPredicateGroup
    //{
    //    public GroupOperator Operator { get; set; }
    //    public IList<IPredicate> Predicates { get; set; }
    //}

    //public enum GroupOperator
    //{
    //    And,
    //    Or
    //}
}