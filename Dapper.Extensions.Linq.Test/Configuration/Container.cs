using Castle.MicroKernel;
using Dapper.Extensions.Linq.CastleWindsor;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Extensions;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Test.Entities;
using Dapper.Extensions.Linq.Core.Repositories;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.Configuration
{

    [TestFixture]
    public class Container
    {
        Castle.Windsor.WindsorContainer _container;

        [SetUp]
        public void RunBeforeAnyTests()
        {
            _container = new Castle.Windsor.WindsorContainer();

            DapperConfiguration
                .Use()
                .UseClassMapper(typeof(AutoClassMapper<>))
                .UseContainer<Dapper.Extensions.Linq.CastleWindsor.WindsorContainer>(c => c.UseExisting(_container))
                .Build();
        }

        [Test]
        public void Automapping_For_Repository_Found()
        {
            object entity = _container.Resolve<IRepository<AutomaticMap>>();
            Assert.NotNull(entity);
        }

        [Test]
        public void Automapping_Not_Found()
        {
            Assert.Throws<ComponentNotFoundException>(() => _container.Resolve<AutomaticMap>());
        }
    }
}