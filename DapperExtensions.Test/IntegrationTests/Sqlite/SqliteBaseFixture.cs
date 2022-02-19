using DapperExtensions.Sql;
using NUnit.Framework;
using System.Data.SQLite;

namespace DapperExtensions.Test.IntegrationTests.Sqlite
{
    [NonParallelizable]
    public class SqliteBaseFixture : PortableDatabaseTestsFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            ConnectionString = ProcessedConnectionString("Sqlite");
            var connection = new SQLiteConnection(ConnectionString);

            CommonSetup(connection, new SqliteDialect());

            ExecuteScripts(connection, true, CreateTableScripts);
        }
    }
}