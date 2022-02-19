using DapperExtensions.Sql;
using NUnit.Framework;
using System.Data.SQLite;

namespace DapperExtensions.Test.IntegrationTests.Async.Sqlite
{
    [NonParallelizable]
    public class SqliteBaseAsyncFixture : PortableDatabaseAsyncTestsFixture
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