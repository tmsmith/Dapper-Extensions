using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;

namespace DapperExtensions
{
    public static class DapperExtensions
    {
        public static IDapperFormatter Formatter { get; set; }
        public static Type DefaultMapper { get; set; }

        private static readonly List<Type> _simpleTypes;
        private static readonly ConcurrentDictionary<Type, IClassMapper> _classMaps = new ConcurrentDictionary<Type, IClassMapper>();

        static DapperExtensions()
        {
            Formatter = new DefaultFormatter();
            DefaultMapper = typeof(AutoClassMapper<>);

            _simpleTypes = new List<Type>();
            _simpleTypes.Add(typeof(byte));
            _simpleTypes.Add(typeof(sbyte));
            _simpleTypes.Add(typeof(short));
            _simpleTypes.Add(typeof(ushort));
            _simpleTypes.Add(typeof(int));
            _simpleTypes.Add(typeof(uint));
            _simpleTypes.Add(typeof(long));
            _simpleTypes.Add(typeof(ulong));
            _simpleTypes.Add(typeof(float));
            _simpleTypes.Add(typeof(double));
            _simpleTypes.Add(typeof(decimal));
            _simpleTypes.Add(typeof(bool));
            _simpleTypes.Add(typeof(string));
            _simpleTypes.Add(typeof(char));
            _simpleTypes.Add(typeof(Guid));
            _simpleTypes.Add(typeof(DateTime));
            _simpleTypes.Add(typeof(DateTimeOffset));
            _simpleTypes.Add(typeof(byte[]));
        }

        public static T Get<T>(this IDbConnection connection, object id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
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
                paramValues = GetObjectValues(id);
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

        public static bool Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Type type = typeof(T);
            IClassMapper classMap = GetMap<T>();
            if (classMap.Properties.Count(c => c.KeyType != KeyType.NotAKey) > 1)
            {
                throw new ArgumentException("Only supporting 1 Key column at this time.");
            }

            string tableName = Formatter.GetTableName(classMap);
            List<string> columns = new List<string>();
            List<string> values = new List<string>();
            PropertyInfo identityProperty = null;
            foreach (var column in classMap.Properties)
            {
                if (column.KeyType == KeyType.Identity)
                {
                    identityProperty = column.PropertyInfo;
                    continue;
                }

                if (column.KeyType == KeyType.Guid && (Guid)column.PropertyInfo.GetValue(entity, null) == Guid.Empty)
                {
                    Guid comb = Formatter.GetNextGuid();
                    column.PropertyInfo.SetValue(entity, comb, null);
                }

                columns.Add(Formatter.GetColumnName(classMap, column, false));
                values.Add("@" + column.Name);
            }


            string sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2});", tableName, columns.AppendStrings(), values.AppendStrings());
            var result = connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text) > 0;
            if (identityProperty != null)
            {
                // TODO: Determine provider and use appropriate method for obtaining identity.
                // SQLCE only supports @@IDENTITY.
                var identityId = connection.Query("SELECT @@IDENTITY AS [Id]", null, transaction, true, commandTimeout, CommandType.Text);

                // TODO: Cast to int required here, Id is a decimal?
                identityProperty.SetValue(entity, (int)identityId.First().Id, null);
            }

            return result;
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

        public static IClassMapper GetMap<T>() where T : class
        {
            Type type = typeof(T);
            IClassMapper map;
            if (!_classMaps.TryGetValue(type, out map))
            {
                Type mapType = type.Assembly.GetTypes()
                    .Where(t => t.GetInterface(typeof(IClassMapper<>).FullName) != null && t.BaseType.GetGenericArguments()[0] == t)
                    .SingleOrDefault();
                if (mapType == null)
                {
                    mapType = DefaultMapper.MakeGenericType(typeof(T));
                }

                map = Activator.CreateInstance(mapType) as IClassMapper;
                _classMaps[type] = map;
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
            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                actualType = type.GetGenericArguments()[0];
            }

            return _simpleTypes.Contains(actualType);
        }

        private static IDictionary<string, object> GetObjectValues(object obj)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            if (obj == null)
            {
                return result;
            }


            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                string name = propertyInfo.Name;
                object value = propertyInfo.GetValue(obj, null);
                if (value == null)
                {
                    continue;
                }

                result[name] = value;
            }

            return result;
        }
    }
}
