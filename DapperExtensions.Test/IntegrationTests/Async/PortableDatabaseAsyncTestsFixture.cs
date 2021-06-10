using DapperExtensions.Test.Helpers;
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests.Async
{
    [Parallelizable(ParallelScope.Self)]
    public abstract class PortableDatabaseAsyncTestsFixture : DatabaseAsyncTestsFixture
    {
        protected PortableDatabaseAsyncTestsFixture(string configPath = null) : base(configPath)
        {
        }

        protected string ProcessedConnectionString(string connectionName)
        {
            return PortableDatabaseHelper.ProcessConnectionString(ConnectionString(connectionName), ProjectPath);
        }

        [TearDown]
        public void TearDown()
        {
            PortableDatabaseHelper.TearDown(Db.Connection.Database);
        }
    }
}