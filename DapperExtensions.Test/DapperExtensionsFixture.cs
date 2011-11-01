using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    public class DapperExtensionsFixture : BaseFixture
    {
        private SqlCeConnection _connection;

        public override void Setup()
        {
            base.Setup();
            if(File.Exists("DapperExtensions.sdf"))
            {
                File.Delete("DapperExtensions.sdf");
            }

            SqlCeEngine ce = new SqlCeEngine(ConfigurationManager.ConnectionStrings["Main"].ConnectionString);
            ce.CreateDatabase();

            _connection = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Main"].ConnectionString);
            _connection.Open();

            SqlCeCommand cmd = new SqlCeCommand(ReadScriptFile("CreatePersonTable"), _connection);
            cmd.ExecuteNonQuery();

        }

        public override void Teardown()
        {
            base.Teardown();
            _connection.Close();
        }

        [Test]
        public void Test()
        {
            
        }
    }
}