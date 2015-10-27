using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Dapper.Extensions.Linq.Core.Sessions;

namespace Dapper.Extensions.Linq.Core.IoC
{
    public class DapperConfiguration : IDapperConfiguration, IDapperConfigurationContainer
    {
        private readonly Dictionary<string, Type> _providers = new Dictionary<string, Type>();
        private Type _assemblyType;
        private IWindsorContainer _container;
        private Func<ComponentRegistration<IDapperSessionContext>, ComponentRegistration<IDapperSessionContext>> _contextRegistrationFunc;

        private DapperConfiguration() { _providers.Add(DapperSessionFactory.DefaultConnectionStringProviderName, typeof(StaticConnectionStringProvider)); }

        /// <summary>
        /// Creates a Dapper configuration with default static <see cref="IConnectionStringProvider"/> and <see cref="IDapperSessionContext"/> per thread.
        /// </summary>
        /// <returns></returns>
        public static IDapperConfiguration Use()
        {
            return new DapperConfiguration();
        }

        public IDapperConfigurationContainer WithContainer(IWindsorContainer container)
        {
            _container = container;
            return this;
        }

        public IDapperConfigurationContainer UsingConnectionProvider(string name, Type provider)
        {
            if (!typeof(IConnectionStringProvider).IsAssignableFrom(provider))
                throw new ArgumentException("provider must implement IConnectionStringProvider");

            if (!_providers.ContainsKey(name))
                _providers.Add(name, provider);

            return this;
        }

        public IDapperConfigurationContainer FromAssemblyContaining(Type assemblyType)
        {
            _assemblyType = assemblyType;
            return this;
        }

        public IDapperConfigurationContainer UseClassMapper(Type typeOf)
        {
            DapperExtensions.DefaultMapper = typeOf;
            return this;
        }

        public void Build()
        {

            if (_assemblyType == null)
                throw new NullReferenceException("FromAssemblyContaining has not been called");

            var registration = Component.For<IDapperSessionContext>()
                .ImplementedBy<DapperSessionContext>();

            registration = _contextRegistrationFunc != null ? _contextRegistrationFunc(registration) : registration.LifestylePerThread();

            _container.Register(Component.For<IDapperSessionFactory>()
                .ImplementedBy<DapperSessionFactory>()
                .DependsOn(Dependency.OnValue<Assembly>(_assemblyType.Assembly)),
                registration);

            if (_providers == null) return;

            foreach (var connectionStringProvider in _providers)
            {
                _container.Register(
                    Component.For<IConnectionStringProvider>()
                        .ImplementedBy(connectionStringProvider.Value)
                        .Named(connectionStringProvider.Key)
                    );
            }
        }

        public IDapperConfigurationContainer UsingContextRegistration(Func<ComponentRegistration<IDapperSessionContext>, ComponentRegistration<IDapperSessionContext>> registrationModifier)
        {
            _contextRegistrationFunc = registrationModifier;
            return this;
        }
    }
}
