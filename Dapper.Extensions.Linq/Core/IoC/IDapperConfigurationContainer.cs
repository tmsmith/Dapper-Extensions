using System;
using Castle.MicroKernel.Registration;
using Dapper.Extensions.Linq.Core.Sessions;

namespace Dapper.Extensions.Linq.Core.IoC
{
    public interface IDapperConfigurationContainer
    {
        IDapperConfigurationContainer UsingConnectionProvider(string name, Type provider);
        IDapperConfigurationContainer UsingContextRegistration(Func<ComponentRegistration<IDapperSessionContext>, ComponentRegistration<IDapperSessionContext>> registrationModifier);
        IDapperConfigurationContainer FromAssemblyContaining(Type assemblyType);
        IDapperConfigurationContainer UseClassMapper(Type typeOf);
        void Build();
    }
}