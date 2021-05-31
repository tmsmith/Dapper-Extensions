using DapperExtensions.Test.Helpers;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading;

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
            return PortableDatabaseHelper.ProcessConnectionString(ConnectionString(connectionName), ProjectPath);
        }

        public static bool IsDatabaseInUse(string databaseName)
        {
            return PortableDatabaseHelper.IsDatabaseInUse(databaseName);
        }

        [TearDown]
        public void TearDown()
        {
            PortableDatabaseHelper.TearDown(Db.Connection.Database);
        }
    }
}