using DapperExtensions.Sql;
using NUnit.Framework;
using System.Data.SqlClient;

namespace DapperExtensions.Test.IntegrationTests.SqlServer
{
    [NonParallelizable]
    public class SqlServerBaseFixture : DatabaseTestsFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            var connection = new SqlConnection(ConnectionString("SqlServer"));

            CommonSetup(connection, new SqlServerDialect());

            ExecuteScripts(connection, true, CreateTableScripts);

            //var files = new List<string>
            //                    {
            //                        ReadFile("CreateAnimalTable"),
            //                        ReadFile("CreateFooTable"),
            //                        ReadFile("CreateMultikeyTable"),
            //                        ReadFile("CreatePersonTable"),
            //                        ReadFile("CreateCarTable")
            //                    };

            //foreach (var setupFile in files)
            //{
            //    connection.Execute(setupFile);
            //}
        }
    }
}