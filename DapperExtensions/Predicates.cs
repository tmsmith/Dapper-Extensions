using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DapperExtensions
{
    public enum DatabaseFunction
    {
        None,
        Truncate,
        NullValue
    }

    public static class Predicates
    {
        /// <summary>
        /// Factory method that creates a new IFieldPredicate predicate: [FieldName] [Operator] [Value]
        /// Example: WHERE FirstName = 'Foo'
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="expression">An expression that returns the left operand [FieldName].</param>
        /// <param name="op">The comparison operator.</param>
        /// <param name="value">The value for the predicate.</param>
        /// <param name="not">Effectively inverts the comparison operator. Example: WHERE FirstName &lt;&gt; 'Foo'.</param>
        /// <param name="useColumPrefix">Indicates to use or not column prefix on generated SQL</param>
        /// <param name="databaseFunction">Apply database function to field</param>
        /// <param name="databaseFunctionParameters">Parameters to the database function</param>
        /// <returns>An instance of IFieldPredicate.</returns>
        public static IFieldPredicate Field<T>(Expression<Func<T, object>> expression, Operator op, object value, bool not = false, bool useColumPrefix = true,
            DatabaseFunction databaseFunction = DatabaseFunction.None, string databaseFunctionParameters = "") where T : class
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;

            return propertyInfo != null ? Field<T>(propertyInfo.Name, op, value, not, useColumPrefix, databaseFunction, databaseFunctionParameters) : null;
        }

        /// <summary>
        /// Factory method that creates a new IFieldPredicate predicate: [FieldName] [Operator] [Value]
        /// Example: WHERE FirstName = 'Foo'
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="propertyName">Property name that returns the left operand [FieldName].</param>
        /// <param name="op">The comparison operator.</param>
        /// <param name="value">The value for the predicate.</param>
        /// <param name="not">Effectively inverts the comparison operator. Example: WHERE FirstName &lt;&gt; 'Foo'.</param>
        /// <param name="useColumPrefix">Indicates to use or not column prefix on generated SQL</param>
        /// <param name="databaseFunction">Apply database function to field</param>
        /// <param name="databaseFunctionParameters">Parameters to the database function</param>
        /// <returns>An instance of IFieldPredicate.</returns>
        public static IFieldPredicate Field<T>(string propertyName, Operator op, object value, bool not = false, bool useColumPrefix = true,
            DatabaseFunction databaseFunction = DatabaseFunction.None, string databaseFunctionParameters = "") where T : class
        {
            IList<PropertyInfo> properties = ReflectionHelper.GetNestedProperties<T>(propertyName, '.', out string propertyInfoName);

            var propertyInfo = typeof(T).GetProperties().SingleOrDefault(x => x.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));

            return new FieldPredicate<T>
            {
                PropertyName = propertyInfo != null ? propertyInfo.Name : propertyInfoName,
                Operator = op,
                Value = value,
                Not = not,
                UseTableAlias = useColumPrefix,
                DatabaseFunction = databaseFunction,
                DatabaseFunctionParameters = databaseFunctionParameters,
                Properties = properties
            };
        }

        /// <summary>
        /// Factory method that creates a new IPropertyPredicate predicate: [FieldName1] [Operator] [FieldName2]
        /// Example: WHERE FirstName = LastName
        /// </summary>
        /// <typeparam name="T">The type of the entity for the left operand.</typeparam>
        /// <typeparam name="T2">The type of the entity for the rigth operand.</typeparam>
        /// <param name="expression">An expression that returns the left operand [FieldName1].</param>
        /// <param name="op">The comparison operator.</param>
        /// <param name="expression2">An expression that returns the rigth operand [FieldName2].</param>
        /// <param name="not">Effectively inverts the comparison operator. Example: WHERE FirstName &lt;&gt; LastName </param>
        /// <param name="useLeftColumPrefix">Indicates to use or not column prefix on left column generated SQL</param>
        /// <param name="useRightColumPrefix">Indicates to use or not column prefix on rigth column generated SQL</param>
        /// <param name="leftDatabaseFunction">Apply database function to field</param>
        /// <param name="leftDatabaseFunctionParameters">Parameters to the database function</param>
        /// <param name="rigthDatabaseFunction">Apply database function to field</param>
        /// <param name="rigthDatabaseFunctionParameters">Parameters to the database function</param>
        /// <returns>An instance of IPropertyPredicate.</returns>
        public static IPropertyPredicate Property<T, T2>(Expression<Func<T, object>> expression, Operator op, Expression<Func<T2, object>> expression2,
            bool not = false, bool useLeftColumPrefix = true, bool useRightColumPrefix = true, DatabaseFunction leftDatabaseFunction = DatabaseFunction.None,
            string leftDatabaseFunctionParameters = "", DatabaseFunction rigthDatabaseFunction = DatabaseFunction.None, string rigthDatabaseFunctionParameters = "")
            where T : class
            where T2 : class
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            var propertyInfo2 = ReflectionHelper.GetProperty(expression2) as PropertyInfo;
            return new PropertyPredicate<T, T2>
            {
                PropertyName = propertyInfo.Name,
                PropertyName2 = propertyInfo2.Name,
                Operator = op,
                Not = not,
                UseTableAlias = useLeftColumPrefix,
                UseRightTableAlias = useRightColumPrefix,
                LeftDatabaseFunction = leftDatabaseFunction,
                LeftDatabaseFunctionParameters = leftDatabaseFunctionParameters,
                RigthDatabaseFunction = rigthDatabaseFunction,
                RigthDatabaseFunctionParameters = rigthDatabaseFunctionParameters
            };
        }

        /// <summary>
        /// Factory method that creates a new IPredicateGroup predicate.
        /// Predicate groups can be joined together with other predicate groups.
        /// </summary>
        /// <param name="op">The grouping operator to use when joining the predicates (AND / OR).</param>
        /// <param name="predicate">A list of predicates to group.</param>
        /// <returns>An instance of IPredicateGroup.</returns>
        public static IPredicateGroup Group(GroupOperator op, params IPredicate[] predicate)
        {
            return new PredicateGroup
            {
                Operator = op,
                Predicates = predicate
            };
        }

        /// <summary>
        /// Factory method that creates a new IExistsPredicate predicate.
        /// </summary>
        /// <param name="predicate">Predicate with the exists comparers</param>
        /// <param name="not">Effectively inverts the comparison operator. Example: WHERE FirstName &lt;&gt; LastName </param>
        public static IExistsPredicate Exists<TSub>(IPredicate predicate, bool not = false)
            where TSub : class
        {
            return new ExistsPredicate<TSub>
            {
                Not = not,
                Predicate = predicate
            };
        }

        /// <summary>
        /// Factory method that creates a new IBetweenPredicate predicate.
        /// </summary>
        /// <param name="expression">Exoression with the property to be used</param>
        /// <param name="values">Values to compare</param>
        /// <param name="not">Effectively inverts the comparison operator. Example: WHERE FirstName &lt;&gt; LastName </param>
        public static IBetweenPredicate Between<T>(Expression<Func<T, object>> expression, BetweenValues values, bool not = false)
            where T : class
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return new BetweenPredicate<T>
            {
                Not = not,
                PropertyName = propertyInfo.Name,
                Value = values
            };
        }

        /// <summary>
        /// Factory method that creates a new Sort which controls how the results will be sorted.
        /// </summary>
        /// <param name="expression">Exoression with the property to be used</param>
        /// <param name="ascending">Indicates if sort must by ascending</param>
        public static ISort Sort<T>(Expression<Func<T, object>> expression, bool ascending = true)
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return new Sort
            {
                PropertyName = propertyInfo.Name,
                Ascending = ascending
            };
        }

        /// <summary>
        /// Factory method that creates a new Sort which controls how the results will be sorted.
        /// </summary>
        /// <param name="propertyName">Property to be used</param>
        /// <param name="ascending">Indicates if sort must by ascending</param>
        public static ISort Sort<T>(string propertyName, bool ascending = true)
        {
            IList<PropertyInfo> propertyInfos = ReflectionHelper.GetNestedProperties<T>(propertyName, '.', out string propertyInfoName);

            return new Sort
            {
                PropertyName = propertyInfoName,
                Ascending = ascending,
                Properties = propertyInfos
            };
        }

        public static IInPredicate In<T>(Expression<Func<T, object>> expression, ICollection collection, bool not = false)
            where T : class
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return new InPredicate<T>(collection, propertyInfo.Name, not);
        }

        /// <summary>
        /// Factory method that creates a Rownum operator
        /// </summary>
        /// <param name="lines">Number of lines to retrieve</param>
        /// <returns></returns>
        public static IRownumPredicate Rownum(long lines)
        {
            return new RownumPredicate { Lines = lines };
        }
    }

    public interface IPredicate
    {
        string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false);
    }

    public interface IBasePredicate : IPredicate
    {
        string PropertyName { get; set; }
    }

    public abstract class BasePredicate : IBasePredicate
    {
        public abstract string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false);
        public string PropertyName { get; set; }

        protected virtual string GetColumnName(Type entityType, ISqlGenerator sqlGenerator, string propertyName, bool isDml = false, bool includePrefix = true)
        {
            var map = sqlGenerator.Configuration.GetMap(entityType);
            if (map == null)
            {
                throw new NullReferenceException(string.Format("Map was not found for {0}", entityType));
            }

            var propertyMap = map.Properties.SingleOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
            if (propertyMap == null)
            {
                throw new NullReferenceException(string.Format("{0} was not found for {1}", propertyName, entityType));
            }

            return sqlGenerator.GetColumnName(map, propertyMap, false, isDml, includePrefix);
        }
    }

    public interface IComparePredicate : IBasePredicate
    {
        Operator Operator { get; set; }
        bool Not { get; set; }
        bool UseTableAlias { get; set; }
    }

    public abstract class ComparePredicate : BasePredicate
    {
        public Operator Operator { get; set; }
        public bool Not { get; set; }
        public bool UseTableAlias { get; set; }

        public virtual string GetOperatorString()
        {
            return Operator switch
            {
                Operator.Gt => Not ? "<=" : ">",
                Operator.Ge => Not ? "<" : ">=",
                Operator.Lt => Not ? ">=" : "<",
                Operator.Le => Not ? ">" : "<=",
                Operator.Like => Not ? "NOT LIKE" : "LIKE",
                Operator.Contains => Not ? "NOT IN" : "IN",
                _ => Not ? "<>" : "=",
            };
        }
    }

    public interface IFieldPredicate : IComparePredicate
    {
        object Value { get; set; }
        DatabaseFunction DatabaseFunction { get; set; }
        string DatabaseFunctionParameters { get; set; }
    }

    public class FieldPredicate<T> : ComparePredicate, IFieldPredicate
        where T : class
    {
        public object Value { get; set; }
        public DatabaseFunction DatabaseFunction { get; set; }
        public string DatabaseFunctionParameters { get; set; }
        public IList<PropertyInfo> Properties { get; set; }

        public FieldPredicate()
        {
            Properties = new List<PropertyInfo>();
        }

        public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            string columnName = string.Empty;
            string propertyName = string.Empty;
            string parameterPropertyName = string.Empty;
            if (Properties.Count > 1)
            {
                propertyName = Properties.Last().Name;
                var parentType = Properties.Last(p => p != Properties.Last()).PropertyType;
                columnName = GetColumnName(parentType, sqlGenerator, propertyName, isDml, UseTableAlias);

                parameterPropertyName = parentType.Name + "_" + propertyName;
            }
            else
            {
                columnName = GetColumnName(typeof(T), sqlGenerator, PropertyName, isDml, UseTableAlias);
            }

            if (!DatabaseFunction.Equals(DatabaseFunction.None))
            {
                columnName = sqlGenerator.Configuration.Dialect.GetDatabaseFunctionString(DatabaseFunction, columnName, DatabaseFunctionParameters);
            }

            if (Value == null)
            {
                return string.Format("({0} IS {1}NULL)", columnName, Not ? "NOT " : string.Empty);
            }

            if (Value is IEnumerable values && !(Value is string))
            {
                if (Operator != Operator.Eq)
                {
                    throw new ArgumentException("Operator must be set to Eq for Enumerable types");
                }

                var @params = new List<string>();
                foreach (var value in values)
                {
                    var p = ReflectionHelper.GetParameter(typeof(T), sqlGenerator, PropertyName, value);
                    var valueParameterName = parameters.SetParameterName(p, sqlGenerator.Configuration.Dialect.ParameterPrefix);
                    @params.Add(valueParameterName);
                }

                var paramStrings = @params.Aggregate(new StringBuilder(), (sb, s) => sb.Append(sb.Length != 0 ? ", " : string.Empty).Append(s), sb => sb.ToString());
                return string.Format("({0} {1}IN ({2}))", columnName, Not ? "NOT " : string.Empty, paramStrings);
            }

            parameterPropertyName = string.IsNullOrEmpty(parameterPropertyName) ? this.PropertyName : parameterPropertyName;

            var propParam = ReflectionHelper.GetParameter(typeof(T), sqlGenerator, parameterPropertyName, this.Value);
            var parameterName = parameters.SetParameterName(propParam, sqlGenerator.Configuration.Dialect.ParameterPrefix);

            if (Operator == Operator.Like && Value != null && sqlGenerator.Configuration.Dialect is OracleDialect)
                return string.Format("(upper({0}) {1} upper('%'||{2}||'%'))", columnName, GetOperatorString(), parameterName);
            else
                return string.Format("({0} {1} {2})", columnName, GetOperatorString(), parameterName);
        }
    }

    public interface IPropertyPredicate : IComparePredicate
    {
        string PropertyName2 { get; set; }
        bool UseRightTableAlias { get; set; }
        DatabaseFunction LeftDatabaseFunction { get; set; }
        string LeftDatabaseFunctionParameters { get; set; }
        DatabaseFunction RigthDatabaseFunction { get; set; }
        string RigthDatabaseFunctionParameters { get; set; }
    }

    public class PropertyPredicate<T, T2> : ComparePredicate, IPropertyPredicate
        where T : class
        where T2 : class
    {
        public string PropertyName2 { get; set; }
        public bool UseRightTableAlias { get; set; }
        public DatabaseFunction LeftDatabaseFunction { get; set; }
        public string LeftDatabaseFunctionParameters { get; set; }
        public DatabaseFunction RigthDatabaseFunction { get; set; }
        public string RigthDatabaseFunctionParameters { get; set; }

        public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            var columnName = GetColumnName(typeof(T), sqlGenerator, PropertyName, false, UseTableAlias);
            var columnName2 = GetColumnName(typeof(T2), sqlGenerator, PropertyName2, false, UseRightTableAlias);

            if (!LeftDatabaseFunction.Equals(DatabaseFunction.None))
            {
                columnName = sqlGenerator.Configuration.Dialect.GetDatabaseFunctionString(LeftDatabaseFunction, columnName, LeftDatabaseFunctionParameters);
            }

            if (!RigthDatabaseFunction.Equals(DatabaseFunction.None))
            {
                columnName2 = sqlGenerator.Configuration.Dialect.GetDatabaseFunctionString(RigthDatabaseFunction, columnName2, RigthDatabaseFunctionParameters);
            }

            return string.Format("({0} {1} {2})", columnName, GetOperatorString(), columnName2);
        }
    }

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
        where T : class
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

    /// <summary>
    /// Comparison operator for predicates.
    /// </summary>
    public enum Operator
    {
        /// <summary>
        /// Equal to
        /// </summary>
        Eq,

        /// <summary>
        /// Greater than
        /// </summary>
        Gt,

        /// <summary>
        /// Greater than or equal to
        /// </summary>
        Ge,

        /// <summary>
        /// Less than
        /// </summary>
        Lt,

        /// <summary>
        /// Less than or equal to
        /// </summary>
        Le,

        /// <summary>
        /// Like (You can use % in the value to do wilcard searching)
        /// </summary>
        Like,

        /// <summary>
        /// Contains a value of an data object.
        /// </summary>
        Contains
    }

    public interface IPredicateGroup : IPredicate
    {
        GroupOperator Operator { get; set; }
        IList<IPredicate> Predicates { get; set; }
    }

    /// <summary>
    /// Groups IPredicates together using the specified group operator.
    /// </summary>
    public class PredicateGroup : IPredicateGroup
    {
        public GroupOperator Operator { get; set; }
        public IList<IPredicate> Predicates { get; set; }
        public string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            string seperator = Operator == GroupOperator.And ? " AND " : " OR ";
            return "(" + Predicates.Aggregate(new StringBuilder(),
                (sb, p) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(p.GetSql(sqlGenerator, parameters, isDml)),
                sb =>
                {
                    var s = sb.ToString();
                    if (s.Length == 0) return sqlGenerator.Configuration.Dialect.EmptyExpression;
                    return s;
                }) + ")";
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

        public string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            var mapSub = GetClassMapper(typeof(TSub), sqlGenerator.Configuration);
            var sql = string.Format("({0}EXISTS (SELECT 1 FROM {1} WHERE {2}))",
                Not ? "NOT " : string.Empty,
                sqlGenerator.GetTableName(mapSub),
                Predicate.GetSql(sqlGenerator, parameters, isDml));
            return sql;
        }

        protected virtual IClassMapper GetClassMapper(Type type, IDapperExtensionsConfiguration configuration)
        {
            var map = configuration.GetMap(type);
            if (map == null)
            {
                throw new NullReferenceException(string.Format("Map was not found for {0}", type));
            }

            return map;
        }
    }

    public interface ISort
    {
        string PropertyName { get; set; }
        bool Ascending { get; set; }
        IList<PropertyInfo> Properties { get; set; }
    }

    public class Sort : ISort
    {
        public string PropertyName { get; set; }
        public bool Ascending { get; set; }
        public IList<PropertyInfo> Properties { get; set; }

        public Sort()
        {
            Properties = new List<PropertyInfo>();
        }
    }

    /// <summary>
    /// Operator to use when joining predicates in a PredicateGroup.
    /// </summary>
    public enum GroupOperator
    {
        And,
        Or
    }

    public interface IProjection
    {
        string PropertyName { get; }
    }

    public class Projection : IProjection
    {
        public Projection(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
    }
    public interface IInPredicate : IPredicate
    {
        ICollection Collection { get; }
        bool Not { get; set; }
    }

    public class InPredicate<T> : BasePredicate, IInPredicate
        where T : class
    {
        public ICollection Collection { get; }
        public bool Not { get; set; }

        public InPredicate(ICollection collection, string propertyName, bool isNot = false)
        {
            PropertyName = propertyName;
            Collection = collection;
            Not = isNot;
        }

        public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            var columnName = GetColumnName(typeof(T), sqlGenerator, PropertyName);

            var @params = new List<string>();

            foreach (var item in Collection)
            {
                var p = ReflectionHelper.GetParameter(typeof(T), sqlGenerator, PropertyName, item);
                @params.Add(parameters.SetParameterName(p, sqlGenerator.Configuration.Dialect.ParameterPrefix));
            }

            var commaDelimited = string.Join(",", @params);

            return $@"({columnName} {GetIsNotStatement(Not)} IN ({commaDelimited}))";
        }

        private static string GetIsNotStatement(bool not)
        {
            return not ? "NOT " : string.Empty;
        }
    }

    public interface IRownumPredicate : IPredicate
    {
        long Lines { get; set; }
    }

    public class RownumPredicate : IRownumPredicate
    {
        public long Lines { get; set; }

        public string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            return $"(ROWNUM <= {Lines})";
        }
    }
}