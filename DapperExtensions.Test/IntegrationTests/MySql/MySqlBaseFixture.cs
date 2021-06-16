using DapperExtensions.Sql;
using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests.MySql
{
    [NonParallelizable]
    public class MySqlBaseFixture : DatabaseTestsFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            var connection = new MySqlConnection(ConnectionString("MySql"));

            CommonSetup(connection, new MySqlDialect());

            ExecuteScripts(Db.Connection, true, CreateTableScripts);
        }
    }
}