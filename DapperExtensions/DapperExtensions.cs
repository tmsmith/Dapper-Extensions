using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;

namespace DapperExtensions
{
    public static class DapperExtensions
    {
        public static bool IsUsingSqlCe { get; set; }
        public static IDapperFormatter Formatter { get; set; }
        public static Type DefaultMapper { get; private set; }

        private static readonly List<Type> _simpleTypes;
        private static readonly ConcurrentDictionary<Type, IClassMapper> _classMaps = new ConcurrentDictionary<Type, IClassMapper>();

        static DapperExtensions()
        {
            Formatter = new DefaultFormatter();
            DefaultMapper = typeof(AutoClassMapper<>);

            _simpleTypes = new List<Type>
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
        }

        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Type type = typeof(T);
            IClassMapper classMap = GetMap<T>();
            if (!classMap.Properties.Any(c => c.KeyType != KeyType.NotAKey))
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }

            if (classMap.Properties.Count(c => c.KeyType != KeyType.NotAKey) > 1)
            {
                throw new ArgumentException("Only supporting 1 Key column at this time.");
            }

            bool isSimpleType = IsSimpleType(id.GetType());
            IDictionary<string, object> paramValues = null;
            if (!isSimpleType)
            {
                paramValues = ReflectionHelper.GetObjectValues(id);
            }
            
            string tableName = Formatter.GetTableName(classMap);
            List<string> columns = new List<string>();
            List<string> where = new List<string>();
            DynamicParameters parameters = new DynamicParameters();
            foreach (var column in classMap.Properties)
            {
                columns.Add(Formatter.GetColumnName(classMap, column, true));

                if (column.KeyType == KeyType.NotAKey)
                {
                    continue;
                }

                where.Add(Formatter.GetColumnName(classMap, column, false) + " = @" + column.Name);
                object value = id;
                if (!isSimpleType)
                {
                    value = paramValues[column.Name];
                }
                
                parameters.Add("@" + column.Name, value);
            }

            string sql = string.Format("SELECT {0} FROM {1} WHERE {2}", columns.AppendStrings(), tableName, where.AppendStrings(" AND "));
            T result = connection.Query<T>(sql, parameters, transaction, true, commandTimeout, CommandType.Text).SingleOrDefault();
            return result;
        }

        public static dynamic Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Type type = typeof(T);
            IClassMapper classMap = GetMap<T>();
            string tableName = Formatter.GetTableName(classMap);
            List<string> columns = new List<string>();
            List<string> values = new List<string>();
            PropertyInfo identityProperty = null;
            IDictionary<string, object> keyValues = new ExpandoObject();
            
            foreach (var column in classMap.Properties)
            {
                if (column.KeyType == KeyType.Identity)
                {
                    if (identityProperty != null)
                    {
                        throw new DataException("Can only set 1 property to Identity.");
                    }

                    identityProperty = column.PropertyInfo;
                    continue;
                }

                if (column.KeyType == KeyType.Guid && (Guid)column.PropertyInfo.GetValue(entity, null) == Guid.Empty)
                {
                    Guid comb = Formatter.GetNextGuid();
                    keyValues.Add(column.Name, comb);
                }

                columns.Add(Formatter.GetColumnName(classMap, column, false));
                values.Add("@" + column.Name);
            }

            string sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2});", tableName, columns.AppendStrings(), values.AppendStrings());
            connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
            if (identityProperty != null)
            {
                string identitySql = string.Format("SELECT IDENT_CURRENT('{0}') AS [Id]", tableName);
                if (IsUsingSqlCe)
                {
                    identitySql = "SELECT @@IDENTITY AS [Id]";
                }

                var identityId = connection.Query(identitySql, null, transaction, true, commandTimeout, CommandType.Text);
                keyValues.Add(identityProperty.Name, (int)identityId.First().Id);
            }

            if (keyValues.Count == 1)
            {
                return keyValues.First().Value;
            }

            return keyValues;
        }

        public static bool Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Type type = typeof(T);
            IClassMapper classMap = GetMap<T>();
            if (!classMap.Properties.Any(c => c.KeyType != KeyType.NotAKey))
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }
            string tableName = Formatter.GetTableName(classMap);
            List<string> columns = new List<string>();
            List<string> where = new List<string>();
            foreach (var column in classMap.Properties)
            {
                string sqlString = string.Format("{0} = @{1}", Formatter.GetColumnName(classMap, column, false), column.Name);
                if (column.KeyType != KeyType.NotAKey)
                {
                    where.Add(sqlString);
                    continue;
                }

                columns.Add(sqlString);
            }

            string sql = string.Format("UPDATE {0} SET {1} WHERE {2};", tableName, columns.AppendStrings(), where.AppendStrings(" AND "));
            return connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text) > 0;
        }

        public static bool Delete<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Type type = typeof(T);
            IClassMapper classMap = GetMap<T>();
            if (!classMap.Properties.Any(c => c.KeyType != KeyType.NotAKey))
            {
                throw new ArgumentException("At least one Key column must be defined.");
            }

            string tableName = Formatter.GetTableName(classMap);
            List<string> where = new List<string>();
            foreach (var column in classMap.Properties)
            {
                string sqlString = string.Format("{0} = @{1}", Formatter.GetColumnName(classMap, column, false), column.Name);
                if (column.KeyType != KeyType.NotAKey)
                {
                    where.Add(sqlString);
                    continue;
                }
            }

            string sql = string.Format("DELETE FROM {0} WHERE {1}", tableName, where.AppendStrings(" AND "));
            return connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text) > 0;
        }

        public static IEnumerable<T> GetList<T>(this IDbConnection connection, IPredicate predicate, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            Type type = typeof(T);
            IClassMapper classMap = GetMap<T>();
            string tableName = Formatter.GetTableName(classMap);
            List<string> columns = classMap.Properties.Select(p => Formatter.GetColumnName(classMap, p, true)).ToList();
            IEnumerable<string> sorts = sort == null ? null : sort.Select(s => Formatter.GetColumnName(classMap, s.PropertyName, false) + (s.Ascending ? " ASC" : " DESC"));

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = string.Format("SELECT {0} FROM {1} WHERE {2}{3}", columns.AppendStrings(), tableName, predicate.GetSql(parameters),
                sorts == null ? string.Empty : " ORDER BY " + sorts.AppendStrings());

            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
        }

        public static int Count<T>(this IDbConnection connection, IPredicate predicate, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            Type type = typeof(T);
            IClassMapper classMap = GetMap<T>();
            string tableName = Formatter.GetTableName(classMap);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string sql = string.Format("SELECT COUNT(*) AS [Total] FROM {0} WHERE {1}", tableName, predicate.GetSql(parameters));
            DynamicParameters dynamicParameters = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                dynamicParameters.Add(parameter.Key, parameter.Value);
            }

            return (int)connection.Query(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text).Single().Total;
        }

        public static IClassMapper GetMap<T>() where T : class
        {
            Type entityType = typeof(T);
            IClassMapper map;
            if (!_classMaps.TryGetValue(entityType, out map))
            {
                Type[] types = entityType.Assembly.GetTypes();
                Type mapType = (from type in types 
                                let interfaceType = type.GetInterface(typeof(IClassMapper<>).FullName) 
                                where interfaceType != null && interfaceType.GetGenericArguments()[0] == entityType 
                                select type).SingleOrDefault();

                if (mapType == null)
                {
                    mapType = DefaultMapper.MakeGenericType(typeof(T));
                }

                map = Activator.CreateInstance(mapType) as IClassMapper;
                _classMaps[entityType] = map;
            }

            return map;
        }

        public static void ClearMapCache()
        {
            _classMaps.Clear();
        }

        private static string AppendStrings(this IEnumerable<string> list, string seperator = ", ")
        {
            return list.Aggregate(
                new StringBuilder(), 
                (sb, s) => (sb.Length == 0 ? sb : sb.Append(seperator)).Append(s),
                sb => sb.ToString());
        }

        private static bool IsSimpleType(Type type)
        {
            Type actualType = type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                actualType = type.GetGenericArguments()[0];
            }

            return _simpleTypes.Contains(actualType);
        }
    }
}
