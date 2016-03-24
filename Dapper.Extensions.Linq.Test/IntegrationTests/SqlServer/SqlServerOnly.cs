using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class SqlServerOnly
    {
        protected Castle.Windsor.WindsorContainer Container;

        [TestFixtureSetUp]
        public void RunBeforeAnyTests()
        {
            Container = SqlSetup.Configuration();
        }
    }
}