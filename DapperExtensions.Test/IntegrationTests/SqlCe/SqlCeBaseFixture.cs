#if NETFRAMEWORK && Windows
using Dapper;
using DapperExtensions.Sql;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.SqlServerCe;

namespace DapperExtensions.Test.IntegrationTests.SqlCe
{
    [NonParallelizable]
    public class SqlCeBaseFixture : PortableDatabaseTestsFixture
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

            ExecuteScripts(Db.Connection, true, CreateTableScripts);
        }
    }
}
#endif