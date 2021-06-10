using DapperExtensions.Sql;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace DapperExtensions.Test.IntegrationTests.Async.SqlServer
{
    [NonParallelizable]
    public class SqlServerBaseAsyncFixture : DatabaseAsyncTestsFixture
    {
        [ExcludeFromCodeCoverage]
        private SqlConnection SetupDatabase()
        {
            var connection = new SqlConnection(ConnectionString("SqlServerDBA"));

            ExecuteScripts(connection, true, "Setup");

            connection.Close();

            return new SqlConnection(ConnectionString("SqlServer"));
        }

        [SetUp]
        public virtual void Setup()
        {
            var connection = new SqlConnection(ConnectionString("SqlServer"));

            try
            {
                CommonSetup(connection, new SqlServerDialect());
            }
            catch (SqlException ex)
            {
                if (ex.Number == 18456)
                {
                    connection = SetupDatabase();
                    CommonSetup(connection, new SqlServerDialect());
                }
                else
                    throw;
            }

            ExecuteScripts(connection, true, CreateTableScripts);
        }
    }
}