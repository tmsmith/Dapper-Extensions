using Castle.Windsor;
using Dapper.Extensions.Linq.Core.Configuration;

namespace Dapper.Extensions.Linq.CastleWindsor
{
    public static class WindsorExtensions
    {
        public static void UseExisting(this ContainerCustomisations customisations, IWindsorContainer container)
        {
            customisations.Settings.Add("ExistingContainer", container);
        }
    }
}