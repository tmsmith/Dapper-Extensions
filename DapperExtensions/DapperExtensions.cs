using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using DapperExtensions.Sql;
using DapperExtensions.Mapper;

namespace DapperExtensions
{
    public static class DapperExtensions
    {
        private readonly static object _lock = new object();

        private static Type _defaultMapper;
        private static Func<Type, ISqlGenerator, IList<Assembly>, IDapperExtensionsImpl> _instanceFactory;
        private static IDapperExtensionsImpl _instance;
        private static ISqlGenerator _sqlGenerator;
        private static IList<Assembly> _mappingAssemblies;
        
        /// <summary>
        /// Gets or sets the default class mapper to use when generating class maps. If not specified, AutoClassMapper<T> is used.
        /// </summary>
        public static Type DefaultMapper
        {
            get
            {
                return _defaultMapper;
            }

            set
            {
                _instance = null;
                _defaultMapper = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of sql to be generated.
        /// </summary>
        public static ISqlDialect SqlDialect
        {
            get
            {
                return _sqlGenerator.Dialect;
            }

            set
            {
                _instance = null;
                _sqlGenerator = new SqlGeneratorImpl(value);
            }
        }

        /// <summary>
        /// Gets the currently setup generator
        /// </summary>
        internal static ISqlGenerator SqlGenerator
        {
            get { return _sqlGenerator; }
        }
        
        /// <summary>
        /// Get or sets the Dapper Extensions Implementation Factory.
        /// </summary>
        public static Func<Type, ISqlGenerator, IList<Assembly>, IDapperExtensionsImpl> InstanceFactory
        {
            get
            {
                if (_instanceFactory == null)
                {
                    _instanceFactory = (dm, sg, ma) => new DapperExtensionsImpl(dm, sg, ma);
                }

                return _instanceFactory;
            }
            set
            {
                _instance = null;
                _instanceFactory = value;
            }
        }

        /// <summary>
        /// Gets the Dapper Extensions Implementation
        /// </summary>
        private static IDapperExtensionsImpl Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = InstanceFactory(DefaultMapper, _sqlGenerator, _mappingAssemblies);
                        }
                    }
                }

                return _instance;
            }
        }

        static DapperExtensions()
        {
            DefaultMapper = typeof(AutoClassMapper<>);
            SqlDialect = new SqlServerDialect();
            _mappingAssemblies = new List<Assembly>();
        }

        public static void SetMappingAssemblies(IList<Assembly> assemblies)
        {
            _mappingAssemblies = assemblies;
            _instance = null;
        }

        //public static void AddMappingAssembly(Assembly assembly)
        //{
        //    _instance = null;
        //    _mappingAssemblies.Add(assembly);
        //}

        //public static void RemoveMappingAssembly(Assembly assembly)
        //{
        //    _instance = null;
        //    _mappingAssemblies.Remove(assembly);
        //}

        /// <summary>
        /// Executes a query for the specified id, returning the data typed as per T
        /// </summary>
        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var result = Instance.Get<T>(connection, id, transaction, commandTimeout);
            return (T)result;
        }

        /// <summary>
        /// Executes an insert query for the specified entity.
        /// </summary>
        public static void Insert<T>(this IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            Instance.Insert<T>(connection, entities, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes an insert query for the specified entity, returning the primary key.  
        /// If the entity has a single key, just the value is returned.  
        /// If the entity has a composite key, an IDictionary&lt;string, object&gt; is returned with the key values.
        /// The key value for the entity will also be updated if the KeyType is a Guid or Identity.
        /// </summary>
        public static dynamic Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Insert<T>(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes an update query for the specified entity.
        /// </summary>
        public static bool Update<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Update<T>(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a delete query for the specified entity.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Delete<T>(connection, entity, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a delete query using the specified predicate.
        /// </summary>
        public static bool Delete<T>(this IDbConnection connection, IPredicate predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Delete<T>(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// </summary>
        public static IEnumerable<T> GetList<T>(this IDbConnection connection, IPredicate predicate = null, IList<ISort> sort = null, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return Instance.GetList<T>(connection, predicate, sort, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a select query using the specified predicate, returning an IEnumerable data typed as per T.
        /// Data returned is dependent upon the specified page and resultsPerPage.
        /// </summary>
        public static IEnumerable<T> GetPage<T>(this IDbConnection connection, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction = null, int? commandTimeout = null, bool buffered = false) where T : class
        {
            return Instance.GetPage<T>(connection, predicate, sort, page, resultsPerPage, transaction, commandTimeout, buffered);
        }

        /// <summary>
        /// Executes a query using the specified predicate, returning an integer that represents the number of rows that match the query.
        /// </summary>
        public static int Count<T>(this IDbConnection connection, IPredicate predicate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            return Instance.Count<T>(connection, predicate, transaction, commandTimeout);
        }

        /// <summary>
        /// Gets the appropriate mapper for the specified type T. 
        /// If the mapper for the type is not yet created, a new mapper is generated from the mapper type specifed by DefaultMapper.
        /// </summary>
        public static IClassMapper GetMap<T>() where T : class
        {
            return Instance.GetMap<T>();
        }

        /// <summary>
        /// Clears the ClassMappers for each type.
        /// </summary>
        public static void ClearCache()
        {
            Instance.ClearCache();
        }

        /// <summary>
        /// Generates a COMB Guid which solves the fragmented index issue.
        /// See: http://davybrion.com/blog/2009/05/using-the-guidcomb-identifier-strategy
        /// </summary>
        public static Guid GetNextGuid()
        {
            return Instance.GetNextGuid();
        }

        public interface IDapperExtensionsImpl
        {
            T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class;
            void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class;
            dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
            bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
            bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class;
            bool Delete<T>(IDbConnection connection, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class;
            IEnumerable<T> GetList<T>(IDbConnection connection, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
            IEnumerable<T> GetPage<T>(IDbConnection connection, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
            int Count<T>(IDbConnection connection, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class;
            IClassMapper GetMap<T>() where T : class;
            void ClearCache();
            Guid GetNextGuid();
        }

        internal class DapperExtensionsImpl : IDapperExtensionsImpl
        {
            private readonly Type _defaultMapper;
            private readonly ISqlGenerator _sqlGenerator;
            private readonly IList<Assembly> _mappingAssemblies;
            private readonly List<Type> _simpleTypes;
            private readonly ConcurrentDictionary<Type, IClassMapper> _classMaps = new ConcurrentDictionary<Type, IClassMapper>();
            
            public DapperExtensionsImpl(Type defaultMapper, ISqlGenerator sqlGenerator, IList<Assembly> mappingAssemblies)
            {
                _defaultMapper = defaultMapper;
                _sqlGenerator = sqlGenerator;
                _mappingAssemblies = mappingAssemblies ?? new List<Assembly>();
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

            public T Get<T>(IDbConnection connection, dynamic id, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                string sql = _sqlGenerator.Get(classMap);
                bool isSimpleType = IsSimpleType(id.GetType());
                IDictionary<string, object> paramValues = null;
                if (!isSimpleType)
                {
                    paramValues = ReflectionHelper.GetObjectValues(id);
                }

                DynamicParameters parameters = new DynamicParameters();
                var keys = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);
                foreach (var key in keys)
                {
                    object value = id;
                    if (!isSimpleType)
                    {
                        value = paramValues[key.Name];
                    }

                    parameters.Add("@" + key.Name, value);
                }


                T result = connection.Query<T>(sql, parameters, transaction, true, commandTimeout, CommandType.Text).SingleOrDefault();
                return result;
            }

            public void Insert<T>(IDbConnection connection, IEnumerable<T> entities, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                var properties = classMap.Properties.Where(p => p.KeyType != KeyType.NotAKey);

                foreach (var e in entities)
                {
                    foreach (var column in properties)
                    {
                        if (column.KeyType == KeyType.Guid)
                        {
                            Guid comb = GetNextGuid();
                            column.PropertyInfo.SetValue(e, comb, null);
                        }
                    }
                }

                string sql = _sqlGenerator.Insert(classMap, false);

                connection.Execute(sql, entities, transaction, commandTimeout, CommandType.Text);
            }

            public dynamic Insert<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                List<IPropertyMap> nonIdentityKeyProperties = classMap.Properties.Where(p => p.KeyType == KeyType.Guid || p.KeyType == KeyType.Assigned).ToList();
                var identityColumn = classMap.Properties.SingleOrDefault(p => p.KeyType == KeyType.Identity);
                foreach (var column in nonIdentityKeyProperties)
                {
                    if (column.KeyType == KeyType.Guid)
                    {
                        Guid comb = GetNextGuid();
                        column.PropertyInfo.SetValue(entity, comb, null);
                    }
                }

                IDictionary<string, object> keyValues = new ExpandoObject();
                string sql = _sqlGenerator.Insert(classMap, true);
                if (identityColumn != null)
                {
                    IEnumerable<int> result;
                    if (_sqlGenerator.RunInsertAsBatch())
                    {
                        result = connection.Query<int>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
                    }
                    else
                    {
                        connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text);
                        sql = _sqlGenerator.IdentitySql(classMap);
                        result = connection.Query<int>(sql, entity, transaction, false, commandTimeout, CommandType.Text);
                    }

                    int identityValue = result.First();
                    keyValues.Add(identityColumn.Name, identityValue);
                    identityColumn.PropertyInfo.SetValue(entity, identityValue, null);
                }
                else
                {
                    connection.Execute(sql, entity, transaction,  commandTimeout, CommandType.Text);
                }

                foreach (var column in nonIdentityKeyProperties)
                {
                    keyValues.Add(column.Name, column.PropertyInfo.GetValue(entity, null));
                }

                if (keyValues.Count == 1)
                {
                    return keyValues.First().Value;
                }

                return keyValues;
            }

            public bool Update<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                string sql = _sqlGenerator.Update(classMap);
                return connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text) > 0;
            }

            public bool Delete<T>(IDbConnection connection, T entity, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                string sql = _sqlGenerator.Delete(classMap);
                return connection.Execute(sql, entity, transaction, commandTimeout, CommandType.Text) > 0;
            }

            public bool Delete<T>(IDbConnection connection, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string sql = _sqlGenerator.Delete(classMap, predicate, parameters);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                return connection.Execute(sql, dynamicParameters, transaction, commandTimeout, CommandType.Text) > 0;
            }
            
            public IEnumerable<T> GetList<T>(IDbConnection connection, IPredicate predicate, IList<ISort> sort, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string sql = _sqlGenerator.GetList(classMap, predicate, sort, parameters);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
            }

            public IEnumerable<T> GetPage<T>(IDbConnection connection, IPredicate predicate, IList<ISort> sort, int page, int resultsPerPage, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string sql = _sqlGenerator.GetPage(classMap, predicate, sort, page, resultsPerPage, parameters);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                return connection.Query<T>(sql, dynamicParameters, transaction, buffered, commandTimeout, CommandType.Text);
            }

            public int Count<T>(IDbConnection connection, IPredicate predicate, IDbTransaction transaction, int? commandTimeout) where T : class
            {
                IClassMapper classMap = GetMap<T>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                string sql = _sqlGenerator.Count(classMap, predicate, parameters);
                DynamicParameters dynamicParameters = new DynamicParameters();
                foreach (var parameter in parameters)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }

                return (int)connection.Query(sql, dynamicParameters, transaction, false, commandTimeout, CommandType.Text).Single().Total;
            }

            public IClassMapper GetMap<T>() where T : class
            {
                Type entityType = typeof(T);
                IClassMapper map;
                if (!_classMaps.TryGetValue(entityType, out map))
                {
                    Type mapType = GetMapType(entityType);
                    if (mapType == null)
                    {
                        mapType = _defaultMapper.MakeGenericType(typeof(T));
                    }

                    map = Activator.CreateInstance(mapType) as IClassMapper;
                    _classMaps[entityType] = map;
                }

                return map;
            }

            public void ClearCache()
            {
                _classMaps.Clear();
            }

            public Guid GetNextGuid()
            {
                byte[] b = Guid.NewGuid().ToByteArray();
                DateTime dateTime = new DateTime(1900, 1, 1);
                DateTime now = DateTime.Now;
                TimeSpan timeSpan = new TimeSpan(now.Ticks - dateTime.Ticks);
                TimeSpan timeOfDay = now.TimeOfDay;
                byte[] bytes1 = BitConverter.GetBytes(timeSpan.Days);
                byte[] bytes2 = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));
                Array.Reverse(bytes1);
                Array.Reverse(bytes2);
                Array.Copy(bytes1, bytes1.Length - 2, b, b.Length - 6, 2);
                Array.Copy(bytes2, bytes2.Length - 4, b, b.Length - 4, 4);
                return new Guid(b);
            }

            protected virtual Type GetMapType(Type entityType)
            {
                Func<Assembly, Type> getType = a =>
                                                   {
                                                       Type[] types = a.GetTypes();
                                                       return (from type in types
                                                               let interfaceType = type.GetInterface(typeof (IClassMapper<>).FullName)
                                                               where
                                                                   interfaceType != null &&
                                                                   interfaceType.GetGenericArguments()[0] == entityType
                                                               select type).SingleOrDefault();
                                                   };

                Type result = getType(entityType.Assembly);
                if (result != null)
                {
                    return result;
                }

                foreach (var mappingAssembly in _mappingAssemblies)
                {
                    result = getType(mappingAssembly);
                    if (result != null)
                    {
                        return result;
                    }
                }

                return getType(entityType.Assembly);
            }

            private bool IsSimpleType(Type type)
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
}
