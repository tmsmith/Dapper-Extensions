using DapperExtensions.Test.Helpers;
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests
{
    [Parallelizable(ParallelScope.Self)]
    public abstract class PortableDatabaseTestsFixture : DatabaseTestsFixture
    {

        protected PortableDatabaseTestsFixture(string configPath = null) : base(configPath)
        {
        }

        protected string ProcessedConnectionString(string connectionName)
        {
            return PortableDatabaseHelper.ProcessConnectionString(GetConnectionString(connectionName), ProjectPath);
        }

        [TearDown]
        public void TearDown()
        {
            PortableDatabaseHelper.TearDown(Db.Connection.Database);
        }
    }
}