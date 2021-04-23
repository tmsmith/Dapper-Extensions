using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;

namespace DapperExtensions
{
    public static class ReflectionHelper
    {
        private static List<Type> _simpleTypes = new List<Type>
            {
                                   typeof(byte),
                                   typeof(sbyte),
                                   typeof(short),
                                   typeof(ushort),
                                   typeof(int),
                                   typeof(uint),
                                   typeof(long),
                                   typeof(ulong),
                                   typeof(float),
                                   typeof(double),
                                   typeof(decimal),
                                   typeof(bool),
                                   typeof(string),
                                   typeof(char),
                                   typeof(Guid),
                                   typeof(DateTime),
                                   typeof(DateTimeOffset),
                                   typeof(byte[])
                               };

        public static MemberInfo GetProperty(LambdaExpression lambda)
        {
            Expression expr = lambda;
            for (; ; )
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Lambda:
                        expr = ((LambdaExpression)expr).Body;
                        break;
                    case ExpressionType.Convert:
                        expr = ((UnaryExpression)expr).Operand;
                        break;
                    case ExpressionType.MemberAccess:
                        MemberExpression memberExpression = (MemberExpression)expr;
                        MemberInfo mi = memberExpression.Member;
                        return mi;
                    default:
                        return null;
                }
            }
        }

        public static IDictionary<string, Func<object>> GetObjectValues(object obj)
        {
            IDictionary<string, Func<object>> result = new Dictionary<string, Func<object>>();
            if (obj == null)
            {
                return result;
            }


            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                string name = propertyInfo.Name;
                Func<object> value = () => propertyInfo.GetValue(obj, null);
                result[name] = value;
            }

            return result;
        }

        public static string AppendStrings(this IEnumerable<string> list, string seperator = ", ")
        {
            return list.Aggregate(
                new StringBuilder(),
                (sb, s) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(s),
                sb => sb.ToString());
        }

        public static bool IsSimpleType(Type type)
        {
            Type actualType = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                actualType = type.GetGenericArguments()[0];
            }

            return _simpleTypes.Contains(actualType);
        }

        public static string GetParameterName(this IDictionary<string, object> parameters, string parameterName, char parameterPrefix)
        {
            return string.Format("{0}{1}_{2}", parameterPrefix, parameterName, parameters.Count);
        }

        public static string SetParameterName(this IDictionary<string, object> parameters, Parameter parameter, char parameterPrefix)
        {
            parameter.Name = parameters.GetParameterName(parameter.ColumnName, parameterPrefix);
            parameters.Add(parameter.Name, parameter);
            return parameter.Name;
        }

        public static Parameter GetParameter(Type entityType, ISqlGenerator sqlGenerator, string propertyName, object value)
        {
            IClassMapper map = sqlGenerator.Configuration.GetMap(entityType);
            if (map == null)
            {
                throw new NullReferenceException(String.Format("Map was not found for {0}", entityType));
            }

            IPropertyMap propertyMap = map.Properties.SingleOrDefault(p => p.Name == propertyName);
            if (propertyMap == null)
            {
                throw new NullReferenceException(String.Format("{0} was not found for {1}", propertyName, entityType));
            }

            return new Parameter
                {
                    ColumnName = propertyMap.ColumnName,
                    DbType = propertyMap.DbType,
                    ParameterDirection = propertyMap.DbDirection,
                    Precision = propertyMap.DbPrecision,
                    Scale = propertyMap.DbScale,
                    Size = propertyMap.DbSize,
                    Value = value,
                    Name = propertyName
                };
        }
    }
}