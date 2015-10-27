using Castle.Windsor;

namespace Dapper.Extensions.Linq.Core.IoC
{
    public interface IDapperConfiguration
    {
        IDapperConfigurationContainer WithContainer(IWindsorContainer container);
    }
}