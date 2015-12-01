using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Core.Sessions;
using Dapper.Extensions.Linq.Core.Sql;

namespace Dapper.Extensions.Linq.Core.Configuration
{
    public class DapperConfiguration : IDapperConfiguration
    {
        private readonly ConcurrentDictionary<Type, IClassMapper> _classMaps = new ConcurrentDictionary<Type, IClassMapper>();
        private IContainer _container;
        internal readonly Dictionary<string, IConnectionStringProvider> Providers;
        public readonly List<Assembly> Assemblies;
        public Type DefaultMapper { get; private set; }
        public string DefaultConnectionStringName { get; private set; }
        public ISqlDialect Dialect { get; private set; }
        public IContainerCustomisations ContainerCustomisations { get; }
        public static IDapperConfiguration Instance { get; private set; }

        private DapperConfiguration()
        {
            ContainerCustomisations = new ContainerCustomisations();
            Providers = new Dictionary<string, IConnectionStringProvider>();
            Assemblies = new List<Assembly>();
            DefaultConnectionStringName = "__Default";
            Instance = null;
        }

        /// <summary>
        /// Creates a Dapper configuration with default static <see cref="IConnectionStringProvider"/> and <see cref="IDapperSessionContext"/> per thread.
        /// </summary>
        /// <returns></returns>
        public static IDapperConfiguration Use()
        {
            return new DapperConfiguration();
        }

        public IDapperConfiguration WithDefaultConnectionStringNamed(string name)
        {
            DefaultConnectionStringName = name;
            return this;
        }

        public IDapperConfiguration UseContainer<T>(Action<IContainerCustomisations> customisations) where T : IContainer
        {
            if (ContainerCustomisations == null)
                throw new Exception("Use method has not beeen called");

            customisations.Invoke(ContainerCustomisations);
            _container = (IContainer)Activator.CreateInstance(typeof(T));

            return this;
        }

        /// <summary>
        /// Assign a class mapper during configuration
        /// 
        /// <see cref="IClassMapper"/>  implemention is required
        /// </summary>
        /// <param name="typeOfMapper"></param>
        /// <returns></returns>
        public IDapperConfiguration UseClassMapper(Type typeOfMapper)
        {
            if (typeof(IClassMapper).IsAssignableFrom(typeOfMapper) == false)
                throw new NullReferenceException("Mapping is not type of IClassMapper");

            DefaultMapper = typeOfMapper;
            return this;
        }
        /// <summary>
        /// Changes the <see cref="ISqlDialect"/>.
        /// </summary>
        /// <param name="dialect"></param>
        /// <returns></returns>
        public IDapperConfiguration UseSqlDialect(ISqlDialect dialect)
        {
            Dialect = dialect;
            return this;
        }

        public IDapperConfiguration UsingConnectionProvider<T>(string name) where T : IConnectionStringProvider
        {
            if (Providers == null)
                throw new Exception("Use method has not beeen called");

            if (typeof(IConnectionStringProvider).IsAssignableFrom(typeof(T)) == false)
                throw new ArgumentException("Provider must implement IConnectionStringProvider");

            if (Providers.ContainsKey(name) == false)
                Providers.Add(name, (IConnectionStringProvider)Activator.CreateInstance(typeof(T)));

            return this;
        }

        public IDapperConfiguration FromAssembly(string name)
        {
            string path = Directory
                .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories)
                .SingleOrDefault(s =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(s);
                    return fileName != null && fileName.Equals(name, StringComparison.OrdinalIgnoreCase);
                });

            if (string.IsNullOrEmpty(path))
                throw new NullReferenceException(string.Format("Assembly '{0}' could not be found.", name));

            string assemblyName = Path.GetFileNameWithoutExtension(path);
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null)
                throw new NullReferenceException(string.Format("Assembly '{0}' could not be loaded.", name));

            Assemblies.Add(assembly);

            return this;
        }

        public IDapperConfiguration FromAssemblyContaining(Type type)
        {
            if (type == null)
                throw new NullReferenceException("Type cannot be null");

            Assemblies.Add(type.Assembly);

            return this;
        }

        public void Build()
        {
            if (Dialect == null)
                throw new NullReferenceException("SqlDialect has not been set. Call UseSqlDialect().");

            if (Providers.Any() == false)
                UsingConnectionProvider<StaticConnectionStringProvider>(DefaultConnectionStringName);

            _container.Build(this);
            Instance = this;
        }

        public IClassMapper GetMap(Type entityType)
        {
            IClassMapper map;
            if (_classMaps.TryGetValue(entityType, out map)) return map;
            Type mapType = GetMapType(entityType) ?? DefaultMapper.MakeGenericType(entityType);

            map = Activator.CreateInstance(mapType) as IClassMapper;
            _classMaps[entityType] = map;

            return map;
        }

        public IClassMapper GetMap<T>() where T : class
        {
            return GetMap(typeof(T));
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
                        let interfaceType = type.GetInterface(typeof(IClassMapper<>).FullName)
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

            foreach (var mappingAssembly in Assemblies)
            {
                result = getType(mappingAssembly);
                if (result != null)
                {
                    return result;
                }
            }

            return getType(entityType.Assembly);
        }
    }
}
