using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Sessions;

namespace Dapper.Extensions.Linq.Core.Configuration
{
    public class DapperConfiguration : IDapperConfigurationContainer
    {
        internal readonly Dictionary<string, IConnectionStringProvider> Providers;
        public List<Assembly> Assemblies;
        private IContainer _container;

        public ContainerCustomisations ContainerCustomisations { get; }

        private DapperConfiguration()
        {
            ContainerCustomisations = new ContainerCustomisations();
            Providers = new Dictionary<string, IConnectionStringProvider>();
            Assemblies = new List<Assembly>();
            UsingConnectionProvider<StaticConnectionStringProvider>(DapperSessionFactory.DefaultConnectionStringProviderName);
        }

        /// <summary>
        /// Creates a Dapper configuration with default static <see cref="IConnectionStringProvider"/> and <see cref="IDapperSessionContext"/> per thread.
        /// </summary>
        /// <returns></returns>
        public static IDapperConfigurationContainer Use()
        {
            return new DapperConfiguration();
        }

        public IDapperConfigurationContainer UseContainer<T>(Action<ContainerCustomisations> customisations) where T : IContainer
        {
            if (ContainerCustomisations == null)
                throw new Exception("Use method has not beeen called");

            customisations.Invoke(ContainerCustomisations);
            _container = (IContainer)Activator.CreateInstance(typeof(T));

            return this;
        }

        public IDapperConfigurationContainer UsingConnectionProvider<T>(string name) where T : IConnectionStringProvider
        {
            if (Providers == null)
                throw new Exception("Use method has not beeen called");

            if (typeof(IConnectionStringProvider).IsAssignableFrom(typeof(T)) == false)
                throw new ArgumentException("Provider must implement IConnectionStringProvider");

            if (Providers.ContainsKey(name) == false)
                Providers.Add(name, (IConnectionStringProvider)Activator.CreateInstance(typeof(T)));

            return this;
        }

        public IDapperConfigurationContainer FromAssembly(string name)
        {
            Assembly assembly = AppDomain
                .CurrentDomain
                .GetAssemblies().
                SingleOrDefault(a => a.GetName().Name == name);

            if (assembly == null)
                throw new NullReferenceException(string.Format("Assembly '{0}' could not be found.", name));

            Assemblies.Add(assembly);

            return this;
        }

        public IDapperConfigurationContainer FromAssemblyContaining(Type type)
        {
            if (type == null)
                throw new NullReferenceException("Type cannot be null");

            Assemblies.Add(type.Assembly);

            return this;
        }

        public void Build()
        {
            if (Assemblies.Any() == false)
                Assemblies = AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .ToList();

            _container.Build(this);
        }
    }
}
