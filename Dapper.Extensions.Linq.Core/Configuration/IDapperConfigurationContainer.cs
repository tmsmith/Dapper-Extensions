using System;
using Dapper.Extensions.Linq.Core.Sessions;

namespace Dapper.Extensions.Linq.Core.Configuration
{
    public interface IDapperConfigurationContainer
    {
        IDapperConfigurationContainer UseContainer<T>(Action<ContainerCustomisations> customisations) where T : IContainer;
        IDapperConfigurationContainer UsingConnectionProvider<T>(string name) where T : IConnectionStringProvider;
        IDapperConfigurationContainer FromAssembly(string name);
        IDapperConfigurationContainer FromAssemblyContaining(Type assemblyType);
        void Build();
    }
}