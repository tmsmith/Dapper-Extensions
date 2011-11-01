using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using DapperExtensions.Test.Data;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    public class DapperExtensionsFixture : BaseFixture
    {
        private SqlCeConnection _connection;
        private readonly string _databaseName;

        public DapperExtensionsFixture()
        {
            _databaseName = ConfigurationManager.AppSettings["DatabaseName"];
        }

        public override void Setup()
        {
            base.Setup();

            if (File.Exists(_databaseName))
            {
                File.Delete(_databaseName);
            }

            string connectionString = "Data Source=" + _databaseName;
            SqlCeEngine ce = new SqlCeEngine(connectionString);
            ce.CreateDatabase();

            _connection = new SqlCeConnection(connectionString);
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
        public void Insert_Person_Inserts_Person_Entity()
        {
            Person p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            _connection.Insert(p);
            Assert.AreEqual(1, p.Id);
        }
    }
}