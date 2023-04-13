using DapperExtensions.Predicate;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperExtensions
{
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
            DatabaseFunction databaseFunction = DatabaseFunction.None, string databaseFunctionParameters = "")
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
            DatabaseFunction databaseFunction = DatabaseFunction.None, string databaseFunctionParameters = "")
        {
            var properties = ReflectionHelper.GetNestedProperties<T>(propertyName, '.', out string propertyInfoName);

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
                Ascending = ascending,
                Properties = ReflectionHelper.GetNestedProperties<T>(propertyInfo.Name, '.', out var _)
            };
        }

        /// <summary>
        /// Factory method that creates a new Sort which controls how the results will be sorted.
        /// </summary>
        /// <param name="propertyName">Property to be used</param>
        /// <param name="ascending">Indicates if sort must by ascending</param>
        public static ISort Sort<T>(string propertyName, bool ascending = true)
        {
            var propertyInfos = ReflectionHelper.GetNestedProperties<T>(propertyName, '.', out var propertyInfoName);

            return new Sort
            {
                PropertyName = propertyInfoName,
                Ascending = ascending,
                Properties = propertyInfos
            };
        }

        public static IInPredicate In<T>(Expression<Func<T, object>> expression, ICollection collection, bool not = false)
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return new InPredicate<T>(collection, propertyInfo.Name, not);
        }

        /// <summary>
        /// Factory method that creates a VirtualPredicate
        /// </summary>
        /// <param name="comparable">What would be used to compare</param>
        /// <param name="op">The operator for the comparison</param>
        /// <param name="value">The value to compare</param>
        /// <param name="not">If it's a not comparison</param>
        /// <example>
        /// VirtualPredicate("Rownum", Operator.Le, 100);
        /// </example>
        /// <returns></returns>
        public static IVirtualPredicate VirtualPredicate(string comparable, Operator op, object value, bool not = false)
        {
            return new VirtualPredicate(comparable, op, value, not);
        }
    }


}