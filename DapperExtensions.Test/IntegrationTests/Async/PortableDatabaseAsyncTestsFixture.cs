using DapperExtensions.Test.Helpers;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace DapperExtensions.Test.IntegrationTests.Async
{
    [Parallelizable(ParallelScope.Self)]
    public abstract class PortableDatabaseAsyncTestsFixture : DatabaseAsyncTestsFixture
    {
        private static readonly object lockObj = new object();

        protected PortableDatabaseAsyncTestsFixture(string configPath = null) : base(configPath)
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