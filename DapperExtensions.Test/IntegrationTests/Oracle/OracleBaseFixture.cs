using DapperExtensions.Sql;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DapperExtensions.Test.IntegrationTests.Oracle
{
    [NonParallelizable]
    public class OracleBaseFixture : DatabaseTestsFixture
    {
        [ExcludeFromCodeCoverage]
        private OracleConnection SetupDatabase()
        {
            var connection = new OracleConnection(ConnectionString("OracleDBA"));

            ExecuteScripts(connection, true, "Setup");

            connection.Close();

            return new OracleConnection(ConnectionString("Oracle"));
        }

        [SetUp]
        public virtual void Setup()
        {
            var connection = new OracleConnection(ConnectionString("Oracle"));

            try
            {
                CommonSetup(connection, new OracleDialect());
            }
            catch (OracleException oex)
            {
                if (oex.Number == 01017)
                {
                    connection = SetupDatabase();
                    CommonSetup(connection, new OracleDialect());
                }
                else
                    throw;
            }

            ExecuteScripts(connection, true, CreateTableScripts.Where(s => s.IndexOf("foo", StringComparison.InvariantCultureIgnoreCase) < 0).ToArray());
        }
    }
}