using DapperExtensions.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DapperExtensions.Predicate
{
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

        protected override string GetColumnName(Type entityType, ISqlGenerator sqlGenerator, string propertyName, bool isDml = false, bool includePrefix = true)
        {
            var result = base.GetColumnName(entityType, sqlGenerator, propertyName, isDml, includePrefix);

            if (!DatabaseFunction.Equals(DatabaseFunction.None))
            {
                result = sqlGenerator.Configuration.Dialect.GetDatabaseFunctionString(DatabaseFunction, result, DatabaseFunctionParameters);
            }

            return result;
        }

        private string GetColumnName(ISqlGenerator sqlGenerator, ref Type parentType, ref string parameterPropertyName, bool isDml)
        {
            string result;
            if (Properties.Count > 1)
            {
                var propertyName = Properties.Last().Name;
                parentType = Properties.Last(p => p != Properties.Last()).PropertyType;
                parameterPropertyName = parentType.Name + "_" + propertyName;

                result = GetColumnName(parentType, sqlGenerator, propertyName, isDml, UseTableAlias);
            }
            else
            {
                result = GetColumnName(typeof(T), sqlGenerator, PropertyName, isDml, UseTableAlias);
            }

            return result;
        }

        private string GetParameterName(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, object value)
        {
            var p = ReflectionHelper.GetParameter(typeof(T), sqlGenerator, PropertyName, value);
            return parameters.SetParameterName(p, sqlGenerator.Configuration.Dialect.ParameterPrefix);
        }

        private string GetParameterName(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string parameterPropertyName, Type parentType)
        {
            parameterPropertyName = string.IsNullOrEmpty(parameterPropertyName) ? PropertyName : parameterPropertyName;

            var propParam = ReflectionHelper.GetParameter(parentType ?? typeof(T), sqlGenerator, parameterPropertyName, Value);
            return parameters.SetParameterName(propParam, sqlGenerator.Configuration.Dialect.ParameterPrefix);
        }

        private string GetParameterString(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, IEnumerable values)
        {
            var @params = new List<string>();
            foreach (var value in values)
                @params.Add(GetParameterName(sqlGenerator, parameters, value));

            return @params.Aggregate(new StringBuilder(), (sb, s) => sb.Append(sb.Length != 0 ? ", " : string.Empty).Append(s), sb => sb.ToString());
        }

        private string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, string columnName, string parameterPropertyName, Type parentType)
        {
            if (Value == null)
                return string.Format("({0} IS {1}NULL)", columnName, Not ? "NOT " : string.Empty);

            if (Value is IEnumerable values && !(Value is string))
            {
                if (Operator != Operator.Eq)
                    throw new ArgumentException("Operator must be set to Eq for Enumerable types");

                return string.Format("({0} {1}IN ({2}))", columnName, Not ? "NOT " : string.Empty, GetParameterString(sqlGenerator, parameters, values));
            }

            var format = (Operator == Operator.Like && Value != null && sqlGenerator.Configuration.Dialect is OracleDialect) ?
                    "(upper({0}) {1} upper('%'||{2}||'%'))" : "({0} {1} {2})";
            if (Operator == Operator.BitEq && Value != null)
            {
                format = (sqlGenerator.Configuration.Dialect is OracleDialect) ? "BITAND({0}, {2}) {1} {2}" : "{0}&{2} {1} {2}";
            }

            return string.Format(format, columnName, GetOperatorString(), GetParameterName(sqlGenerator, parameters, parameterPropertyName, parentType));
        }

        public override string GetSql(ISqlGenerator sqlGenerator, IDictionary<string, object> parameters, bool isDml = false)
        {
            var parameterPropertyName = string.Empty;
            Type parentType = null;

            var columnName = GetColumnName(sqlGenerator, ref parentType, ref parameterPropertyName, isDml);

            return GetSql(sqlGenerator, parameters, columnName, parameterPropertyName, parentType);
        }
    }
}
