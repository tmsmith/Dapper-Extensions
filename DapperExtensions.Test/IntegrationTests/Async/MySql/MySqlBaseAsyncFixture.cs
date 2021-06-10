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
            var connection = new MySqlConnection(ConnectionString("MySql"));

            CommonSetup(connection, new MySqlDialect());

            ExecuteScripts(connection, true, CreateTableScripts);
        }
    }
}