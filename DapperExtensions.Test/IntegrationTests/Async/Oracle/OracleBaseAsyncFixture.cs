using DapperExtensions.Sql;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System.Linq;

namespace DapperExtensions.Test.IntegrationTests.Async.Oracle
{
    [NonParallelizable]
    public class OracleBaseAsyncFixture : DatabaseAsyncTestsFixture
    {
        [SetUp]
        public virtual void Setup()
        {
            var connection = new OracleConnection(ConnectionString("Oracle"));

            CommonSetup(connection, new OracleDialect());

            ExecuteScripts(connection, true, CreateTableScripts.Where(s => s.IndexOf("foo", System.StringComparison.InvariantCultureIgnoreCase) < 0).ToArray());

            //var files = new List<string>
            //                    {
            //                        ReadFile("CreateAnimalTable"),
            //                        ReadFile("CreatePersonTable"),
            //                        ReadFile("CreateCarTable"),
            //                        ReadFile("CreateMultikeyTable")
            //                    };

            //foreach (var setupFile in files)
            //{
            //    connection.Execute(setupFile);
            //}
        }
    }
}