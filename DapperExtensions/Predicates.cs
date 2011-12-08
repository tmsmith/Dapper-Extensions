using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DapperExtensions
{
    public static class Predicates
    {
        public static IFieldPredicate Field<T>(Expression<Func<T, object>> expression, Operator op, object value, bool not = false) where T : class
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

        public static IPropertyPredicate Property<T, T2>(Expression<Func<T, object>> expression, Operator op, Expression<Func<T2, object>> expression2, bool not = false) 
            where T : class
            where T2 : class
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            PropertyInfo propertyInfo2 = ReflectionHelper.GetProperty(expression2) as PropertyInfo;
            return new PropertyPredicate<T, T2>
                       {
                           PropertyName = propertyInfo.Name,
                           PropertyName2 = propertyInfo2.Name,
                           Operator = op,
                           Not = not
                       };
        }

        public static IPredicateGroup Group(GroupOperator op, params IPredicate[] predicate)
        {
            return new PredicateGroup
                       {
                           Operator = op,
                           Predicates = predicate
                       };
        }

        public static IExistsPredicate Exists<TSub>(IPredicate predicate, bool not = false)
            where TSub : class
        {
            return new ExistsPredicate<TSub>
                       {
                           Not = not,
                           Predicate = predicate
                       };
        }

        public static ISort Sort<T>(Expression<Func<T, object>> expression, bool ascending = true)
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return new Sort
                       {
                           PropertyName = propertyInfo.Name,
                           Ascending = ascending
                       };
        }
    }

    public interface IPredicate
    {
        string GetSql(IDictionary<string, object> parameters);
    }

    public interface IComparePredicate : IPredicate
    {
        string PropertyName { get; set; }
        Operator Operator { get; set; }
    }

    public abstract class ComparePredicate : IComparePredicate
    {
        public string PropertyName { get; set; }
        public Operator Operator { get; set; }
        public bool Not { get; set; }

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
                case Operator.Like:
                    return Not ? "NOT LIKE" : "LIKE";
                default:
                    return Not ? "<>" : "=";
            }
        }

        protected string GetColumnName<T>(string propertyName) where T : class
        {
            IClassMapper map = DapperExtensions.GetMap<T>();
            if (map == null)
            {
                throw new NullReferenceException(string.Format("Map was not found for {0}", typeof(T)));
            }

            IPropertyMap propertyMap = map.Properties.Single(p => p.Name == propertyName);
            if (map == null)
            {
                throw new NullReferenceException(string.Format("{0} was not found for {1}", propertyName, typeof(T)));
            }

            return DapperExtensions.SqlGenerator.GetColumnName(map, propertyMap, false);
        }

        public abstract string GetSql(IDictionary<string, object> parameters);
    }

    public interface IFieldPredicate : IComparePredicate
    {
        object Value { get; set; }
    }

    public class FieldPredicate<T> : ComparePredicate, IFieldPredicate 
        where T : class
    {
        public object Value { get; set; }

        public override string GetSql(IDictionary<string, object> parameters)
        {
            string columnName = GetColumnName<T>(PropertyName);
            if (Value == null)
            {
                return string.Format("({0} IS {1}NULL)", columnName, Not ? "NOT " : string.Empty);
            }

            if (Value is IEnumerable && !(Value is string))
            {
                if (Operator != Operator.Eq)
                {
                    throw new ArgumentException("Operator must be set to Eq for Enumerable types");
                }

                List<string> @params = new List<string>();
                foreach (var value in (IEnumerable)Value)
                {
                    string valueParameterName = string.Format("@{0}_{1}", PropertyName, parameters.Count);
                    parameters.Add(valueParameterName, value);
                    @params.Add(valueParameterName);
                }

                string paramStrings = @params.Aggregate(new StringBuilder(), (sb, s) => sb.Append((sb.Length != 0 ? ", " : string.Empty) + s), sb => sb.ToString());
                return string.Format("({0} {1}IN ({2}))", columnName, Not ? "NOT " : string.Empty, paramStrings);
            }

            string parameterName = string.Format("@{0}_{1}", PropertyName, parameters.Count);
            parameters.Add(parameterName, Value);
            return string.Format("({0} {1} {2})", columnName, GetOperatorString(), parameterName);
        }
    }

    public interface IPropertyPredicate : IComparePredicate
    {
        string PropertyName2 { get; set; }
    }

    public class PropertyPredicate<T, T2> : ComparePredicate, IPropertyPredicate
        where T : class
        where T2 : class
    {
        public string PropertyName2 { get; set; }

        public override string GetSql(IDictionary<string, object> parameters)
        {
            string columnName = GetColumnName<T>(PropertyName);
            string columnName2 = GetColumnName<T2>(PropertyName2);
            return string.Format("({0} {1} {2})", columnName, GetOperatorString(), columnName2);
        }
    }

    public enum Operator
    {
        Eq,
        Gt,
        Ge,
        Lt,
        Le,
        Like
    }

    public interface IPredicateGroup : IPredicate
    {
        GroupOperator Operator { get; set; }
        IList<IPredicate> Predicates { get; set; }
    }

    public class PredicateGroup : IPredicateGroup
    {
        public GroupOperator Operator { get; set; }
        public IList<IPredicate> Predicates { get; set; }
        public string GetSql(IDictionary<string, object> parameters)
        {
            string seperator = Operator == GroupOperator.And ? " AND " : " OR ";
            return "(" + Predicates.Aggregate(new StringBuilder(),
                                        (sb, p) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(p.GetSql(parameters)),
                                        sb => sb.ToString()) + ")";
        }
    }

    public interface IExistsPredicate : IPredicate
    {
        IPredicate Predicate { get; set; }
        bool Not { get; set; }
    }

    public class ExistsPredicate<TSub> : IExistsPredicate
        where TSub : class
    {
        public IPredicate Predicate { get; set; }
        public bool Not { get; set; }

        public string GetSql(IDictionary<string, object> parameters)
        {
            IClassMapper mapSub = GetClassMapper<TSub>();
            string sql = string.Format("({0}EXISTS (SELECT 1 FROM {1} WHERE {2}))", 
                Not ? "NOT " : string.Empty,
                DapperExtensions.SqlGenerator.GetTableName(mapSub), 
                Predicate.GetSql(parameters));
            return sql;
        }

        protected IClassMapper GetClassMapper<T>() where T : class
        {
            IClassMapper map = DapperExtensions.GetMap<T>();
            if (map == null)
            {
                throw new NullReferenceException(string.Format("Map was not found for {0}", typeof(T)));
            }

            return map;
        }
    }

    public interface ISort
    {
        string PropertyName { get; set; }
        bool Ascending { get; set; }
    }

    public class Sort : ISort
    {
        public string PropertyName { get; set; }
        public bool Ascending { get; set; }
    }

    public enum GroupOperator
    {
        And,
        Or
    }
}