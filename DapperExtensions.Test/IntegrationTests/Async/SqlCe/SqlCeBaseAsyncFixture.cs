#if NET461
using Dapper;
using DapperExtensions.Sql;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.SqlServerCe;

namespace DapperExtensions.Test.IntegrationTests.Async.SqlCe
{
    [NonParallelizable]
    public class SqlCeBaseFixture : PortableDatabaseAsyncTestsFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            var cs = ProcessedConnectionString("SqlCe");
            using (SqlCeEngine ce = new SqlCeEngine(cs))
            {
                ce.CreateDatabase();
            }

            SqlCeConnection connection = new SqlCeConnection(cs);

            CommonSetup(connection, new SqlCeDialect());

            ExecuteScripts(connection, true, CreateTableScripts);
        }
    }
}
#endif