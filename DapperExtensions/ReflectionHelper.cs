using DapperExtensions.Enums;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace DapperExtensions
{
    public static class ReflectionHelper
    {
        private static readonly List<Type> _simpleTypes = new List<Type>
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

        public static IList<PropertyInfo> GetNestedProperties<T>(string nestedProperties, char delimiter, out string propertyInfoName)
        {
            IList<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            var properties = nestedProperties.Split(delimiter);
            string _propertyInfoName = "";

            var parentType = typeof(T);
            int index = 0;
            foreach (var propName in properties)
            {
                index++;
                var propertyInfo = GetPropertyInfo(parentType, propName);
                propertyInfos.Add(propertyInfo);

                _propertyInfoName += propertyInfo.Name + ((index < properties.Length) ? delimiter.ToString() : "");

                parentType = propertyInfo.PropertyType;
            }
            propertyInfoName = _propertyInfoName;

            return propertyInfos;
        }

        public static object GetProperty(Expression lambda, bool isMapping = false)
        {
            IList<MemberInfo> memberInfos = new List<MemberInfo>();
            Expression expr = lambda;
            for (; ; )
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Lambda:
                        expr = ((LambdaExpression)expr).Body;

                        if (isMapping)
                        {
                            switch (expr.NodeType)
                            {
                                case ExpressionType.New:
                                    NewExpression newExpression = (NewExpression)expr;
                                    IList<MemberInfo> members = newExpression.Members;
                                    return members;
                                case ExpressionType.Convert:
                                    UnaryExpression unary = ((UnaryExpression)expr);
                                    return unary;
                            }
                        }

                        break;
                    case ExpressionType.Convert:
                        expr = ((UnaryExpression)expr).Operand;
                        break;
                    case ExpressionType.MemberAccess:
                        MemberExpression memberExpression = (MemberExpression)expr;
                        MemberInfo mi = memberExpression.Member;

                        if (memberExpression.Expression is MemberExpression)
                        {
                            memberInfos.Insert(0, mi);
                            expr = memberExpression.Expression;

                            break;
                        }

                        if (memberInfos.Count > 0)
                        {
                            memberInfos.Insert(0, mi);
                            return memberInfos;
                        }

                        return mi;

                    case ExpressionType.Call:
                        return ((MethodCallExpression)expr).Arguments;
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
                if (propertyInfo.GetIndexParameters().Length > 0) continue;
                string name = propertyInfo.Name;
                object value() => propertyInfo.GetValue(obj, null);
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

        public static void SetValue(Type type, string propertyName, object obj, object value)
        {
            var property = type.GetProperty(propertyName);
            property.SetValue(obj, value, null);
        }

        public static BinaryExpression GetBinaryExpressions(BinaryExpression expression, out IList<BinaryExpression> binaries)
        {
            BinaryExpression binary = expression;
            IList<BinaryExpression> _binaries = new List<BinaryExpression>();

            if (binary.Left is BinaryExpression left)
                _binaries.Add(GetBinaryExpressions(left, out binaries));

            if (binary.Right is BinaryExpression right)
                _binaries.Add(GetBinaryExpressions(right, out binaries));
            else
                if (!_binaries.Any(b => b == binary))
                _binaries.Add(binary);

            binaries = _binaries;
            return binary;
        }

        public static IList<BinaryExpression> GetBinaryExpressionsFromUnary(UnaryExpression expression)
        {
            UnaryExpression unary = ((UnaryExpression)expression);
            Expression operand = ((Expression)unary.Operand);
            BinaryExpression binary = (BinaryExpression)operand;
            IList<BinaryExpression> lBinaries = new List<BinaryExpression>();
            IList<BinaryExpression> rBinaries = new List<BinaryExpression>();

            if (binary.Left != null)
            {
                if (binary.Left is BinaryExpression left)
                    GetBinaryExpressions(left, out lBinaries);
                else if (!lBinaries.Any(b => b == binary) && !rBinaries.Any(b => b == binary))
                    lBinaries.Add(binary);
            }

            if (binary.Right != null)
            {
                if (binary.Right is BinaryExpression right)
                    GetBinaryExpressions(right, out rBinaries);
                else if (!lBinaries.Any(b => b == binary) && !rBinaries.Any(b => b == binary))
                    rBinaries.Add(binary);
            }

            return lBinaries.Concat(rBinaries).ToList();
        }

        public static Comparator GetRelacionalComparator(ExpressionType type, string name = "")
        {
            return type switch
            {
                ExpressionType.Equal => Comparator.Equal,
                ExpressionType.NotEqual => Comparator.NotEqual,
                ExpressionType.GreaterThan => Comparator.GreaterThan,
                ExpressionType.LessThan => Comparator.LessThan,
                ExpressionType.GreaterThanOrEqual => Comparator.GreaterThanOrEqual,
                ExpressionType.LessThanOrEqual => Comparator.LessThanOrEqual,
                _ => throw new ArgumentOutOfRangeException(string.Format("Comparator option is not valid for property {0} detected.", name)),
            };
        }

        private static AssemblyBuilder CreateAssemblyBuilder(AssemblyName assemblyName)
        {
            return AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        }

        public static AssemblyBuilder CreateAssemblyBuilder(string assemblyName)
        {
            AssemblyName name = new AssemblyName(assemblyName);
            return CreateAssemblyBuilder(name);
        }

        public static ModuleBuilder CreateModuleBuilder(AssemblyBuilder assemblyBuilder, string moduleName)
        {
            return assemblyBuilder
                .DefineDynamicModule(moduleName);
        }

        public static TypeBuilder CreateTypeBuilder(ModuleBuilder moduleBuilder, string typeName, Type baseType = null)
        {
            string _typeName = string.Format("{0}{1}", typeName, DapperExtensions.GetNextGuid().ToString().Substring(0, 8));
            return moduleBuilder
                .DefineType(_typeName, TypeAttributes.Public |
                              TypeAttributes.Class |
                              TypeAttributes.AutoClass |
                              TypeAttributes.AnsiClass |
                              TypeAttributes.BeforeFieldInit |
                              TypeAttributes.AutoLayout, baseType);
        }

        public static Type CreateVirtualType(TypeBuilder typeBuilder, Type entityType)
        {
            var properties = entityType.GetProperties();

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            foreach (var property in properties)
                typeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, null);

#if NETSTANDARD2_0
            return typeBuilder.CreateTypeInfo();
#else
            return typeBuilder.CreateType();
#endif
        }

        public static Type CreateMapType(TypeBuilder typeBuilder, Type entityType, Type extendedType)
        {
            var baseType = typeof(Mapper.ClassMapper<>);
            Type[] baseTypeArgs = { entityType };
            baseType = baseType.MakeGenericType(baseTypeArgs);

            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            Type[] binders = { typeof(Mapper.ClassMapper<>) };
            var ctors = extendedType.GetConstructor(binders);

            foreach (var property in extendedType.GetProperties())
            {
                typeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, null);
            }

            foreach (var method in extendedType.GetMethods().Where(c => c.Name == "Table"))
            {
                if (method.Name == "Table")
                {
                    MethodBuilder methodBuilder = typeBuilder.DefineMethod(method.Name, method.Attributes);
                    foreach (var parameter in method.GetParameters())
                    {
                        methodBuilder.DefineParameter(parameter.Position, parameter.Attributes, parameter.Name);
                    }

                    var methodIl = methodBuilder.GetILGenerator();

                    methodIl.Emit(OpCodes.Ldarg_0);
                    methodIl.Emit(OpCodes.Conv_I8);
                    methodIl.Emit(OpCodes.Dup);
                    methodIl.Emit(OpCodes.Mul);
                    methodIl.Emit(OpCodes.Ret);
                }
            }
#if NETSTANDARD2_0
            return typeBuilder.CreateTypeInfo();
#else
            return typeBuilder.CreateType();
#endif
        }

        private static object GetParameterValue(object value)
        {
            return value is Func<object> ? (value as Func<object>).Invoke() : value;
        }

        public static Parameter GetParameter(Type entityType, ISqlGenerator sqlGenerator, string propertyName, object value)
        {
            IClassMapper map = sqlGenerator.Configuration.GetMap(entityType);
            if (map == null)
                throw new NullReferenceException(String.Format("Map was not found for {0}", entityType));

            var entityPropertyName = propertyName.Split('_').Last();

            IMemberMap propertyMap = map.Properties.SingleOrDefault(p => p.Name == entityPropertyName);
            if (propertyMap == null)
                throw new NullReferenceException(String.Format("{0} was not found for {1}", entityPropertyName, entityType));

            return new Parameter
            {
                ColumnName = propertyMap.ColumnName,
                DbType = propertyMap.DbType,
                ParameterDirection = propertyMap.DbDirection,
                Precision = propertyMap.DbPrecision,
                Scale = propertyMap.DbScale,
                Size = propertyMap.DbSize,
                Value = GetParameterValue(value),
                Name = propertyName
            };
        }

        public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            if (!(type.GetProperties().SingleOrDefault(x => x.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)) is PropertyInfo propertyInfo))
            {
                throw new System.Exception($"Property name '{propertyName}' not exists inside {type.FullName}. \n" +
                    "Error on class: 'DapperPredicatesWrapper' - method: 'Sort'.");
            }

            return propertyInfo;
        }
    }
}