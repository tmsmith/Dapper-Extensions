using System;
using Castle.MicroKernel.Registration;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Core.Sessions;
using Dapper.Extensions.Linq.Repositories;

namespace Dapper.Extensions.Linq.CastleWindsor
{
    public class WindsorContainer : IContainer
    {
        public void Build(DapperConfiguration configuration)
        {
            object container;
            bool success = configuration.ContainerCustomisations.Settings().TryGetValue("ExistingContainer", out container);
            if (success == false) throw new NullReferenceException("ExistingContainer not found");

            var windsorContainer = (Castle.Windsor.WindsorContainer)container;
            var sessionContext = Component.For<IDapperSessionContext>()
                .ImplementedBy<DapperSessionContext>()
                .LifestylePerThread();

            windsorContainer.Register(Component.For(typeof(IRepository<>))
                .ImplementedBy(typeof(DapperRepository<>))
                .LifestylePerThread());

            foreach (var assembly in configuration.Assemblies)
            {
                windsorContainer.Register(
                    Classes.FromAssembly(assembly)
                    .BasedOn(typeof(DapperRepository<>))
                    .LifestylePerThread());
            }

            windsorContainer.Register(Component.For<IDapperSessionFactory>()
                .ImplementedBy<DapperSessionFactory>()
                .DependsOn(Dependency.OnValue<DapperConfiguration>(configuration)),
                sessionContext);
        }
    }
}