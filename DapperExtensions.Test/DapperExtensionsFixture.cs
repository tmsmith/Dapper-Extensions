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

        [Test]
        public void Get_Person_Gets_Person_Entity()
        {
            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            _connection.Insert(p1);

            Person p2 = _connection.Get<Person>(p1.Id);
            Assert.AreEqual(p1.Id, p2.Id);
            Assert.AreEqual("Foo", p2.FirstName);
            Assert.AreEqual("Bar", p2.LastName);
        }

        [Test]
        public void Delete_Person_Deletes_Person_Entity()
        {
            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            _connection.Insert(p1);

            Assert.AreNotEqual(0, p1);

            _connection.Delete(p1);
            Assert.IsNull(_connection.Get<Person>(p1.Id));

        }

        [Test]
        public void Update_Person_Updates_Person_Entity()
        {
            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            _connection.Insert(p1);

            Assert.AreNotEqual(0, p1);

            var p2 = _connection.Get<Person>(p1.Id);
            p2.FirstName = "Baz";
            p2.Active = false;

            _connection.Update(p2);

            var p3 = _connection.Get<Person>(p1.Id);
            Assert.AreEqual("Baz", p3.FirstName);
            Assert.AreEqual("Bar", p3.LastName);
            Assert.AreEqual(false, p3.Active);

        }
    }
}