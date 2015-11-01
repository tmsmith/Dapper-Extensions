using System;
using Castle.MicroKernel.Registration;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Core.Sessions;

namespace Dapper.Extensions.Linq.CastleWindsor
{
    public class WindsorContainer : IContainer
    {
        public void Build(DapperConfiguration configuration)
        {
            object container;
            bool success = configuration.ContainerCustomisations.Settings.TryGetValue("ExistingContainer", out container);
            if (success == false) throw new NullReferenceException("ExistingContainer not found");

            var windsorContainer = (Castle.Windsor.WindsorContainer)container;
            var registration = Component.For<IDapperSessionContext>()
                .ImplementedBy<DapperSessionContext>();

            //session context
            registration = registration.LifestylePerThread();

            //session factory
            windsorContainer.Register(Component.For<IDapperSessionFactory>()
                .ImplementedBy<DapperSessionFactory>()
                .DependsOn(Dependency.OnValue<DapperConfiguration>(configuration)),
                registration);
        }
    }
}