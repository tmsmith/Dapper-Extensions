using DapperExtensions.Sql;
using NUnit.Framework;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace DapperExtensions.Test.IntegrationTests.SqlServer
{
    [NonParallelizable]
    public class SqlServerBaseFixture : DatabaseTestsFixture
    {
        [ExcludeFromCodeCoverage]
        private SqlConnection SetupDatabase()
        {
            var connection = new SqlConnection(GetConnectionString("SqlServerDBA"));

            ExecuteScripts(connection, true, "Setup");

            connection.Close();

            return new SqlConnection(GetConnectionString("SqlServer"));
        }

        [SetUp]
        public virtual void Setup()
        {
            ConnectionString = GetConnectionString("SqlServer");
            var connection = new SqlConnection(ConnectionString);

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