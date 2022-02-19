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
            ConnectionString = GetConnectionString("MySql");
            var connection = new MySqlConnection(ConnectionString);

            CommonSetup(connection, new MySqlDialect());

            ExecuteScripts(Db.Connection, true, CreateTableScripts);
        }
    }
}