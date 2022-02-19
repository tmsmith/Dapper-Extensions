using DapperExtensions.Sql;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests.Async.MySql
{
    [NonParallelizable]
    public class MySqlBaseAsyncFixture : DatabaseAsyncTestsFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            ConnectionString = GetConnectionString("MySql");
            var connection = new MySqlConnection(ConnectionString);

            CommonSetup(connection, new MySqlDialect());

            ExecuteScripts(connection, true, CreateTableScripts);
        }
    }
}