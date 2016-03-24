using System;
using System.Linq.Expressions;
using Castle.Windsor;
using Dapper.Extensions.Linq.Builder;
using Dapper.Extensions.Linq.CastleWindsor;
using Dapper.Extensions.Linq.Core.Configuration;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.SQLite;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.Fixtures
{
    public class QueryBuilderTest
    {
        private IWindsorContainer Container { get; set; }

        [TestFixtureSetUp]
        public void RunBeforeAnyTests()
        {
            Container = new Castle.Windsor.WindsorContainer();

            DapperConfiguration
                .Use()
                .UseClassMapper(typeof(AutoClassMapper<>))
                .UseContainer<ContainerForWindsor>(cfg => cfg.UseExisting(Container))
                .UseSqlDialect(new SQLiteDialect())
                .FromAssembly("Dapper.Extensions.Linq.Test.Entities")
                .Build();
        }

        [Test]
        public void QueryBuilder_BooleanTrue()
        {
            Expression<Func<Person, bool>> expression = e => e.Id == 14 && e.Active;

            QueryBuilder<Person>.FromExpression(expression);
        }

        [Test]
        public void QueryBuilder_LeftSideMemberExpression()
        {
            Expression<Func<Person, bool>> expression = e => DateTime.Now == e.DateCreated && e.Active;

            QueryBuilder<Person>.FromExpression(expression);
        }
    }
}
