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

            //var files = new List<string>
            //    {
            //        ReadFile("CreateFooTable"),
            //        ReadFile("CreateMultikeyTable"),
            //        ReadFile("CreatePersonTable"),
            //        ReadFile("CreateCarTable"),
            //        ReadFile("CreateAnimalTable")
            //    };

            //foreach (var setupFile in files)
            //{
            //    connection.Execute(setupFile);
            //}
        }
    }
}