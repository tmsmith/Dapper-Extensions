using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
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
            DapperExtensions.IsUsingSqlCe = true;
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
            int id = _connection.Insert(p);
            Assert.AreEqual(1, id);
        }

        [Test]
        public void Get_Person_Gets_Person_Entity()
        {
            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            int id = _connection.Insert(p1);

            Person p2 = _connection.Get<Person>(id);
            Assert.AreEqual(id, p2.Id);
            Assert.AreEqual("Foo", p2.FirstName);
            Assert.AreEqual("Bar", p2.LastName);
        }

        [Test]
        public void Delete_Person_Deletes_Person_Entity()
        {
            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            int id = _connection.Insert(p1);

            Person p2 = _connection.Get<Person>(id);
            _connection.Delete(p2);
            Assert.IsNull(_connection.Get<Person>(id));
        }

        [Test]
        public void Update_Person_Updates_Person_Entity()
        {
            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            int id = _connection.Insert(p1);

            var p2 = _connection.Get<Person>(id);
            p2.FirstName = "Baz";
            p2.Active = false;

            _connection.Update(p2);

            var p3 = _connection.Get<Person>(id);
            Assert.AreEqual("Baz", p3.FirstName);
            Assert.AreEqual("Bar", p3.LastName);
            Assert.AreEqual(false, p3.Active);
        }

        [Test]
        public void GetList_With_Predicates_Returns_Correct_Items()
        {
            _connection.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            _connection.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            _connection.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            _connection.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

            var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
            IEnumerable<Person> list = _connection.GetList<Person>(predicate);
            Assert.AreEqual(2, list.Count());
            var a = list.Single(p => p.FirstName == "a");
            Assert.AreEqual("a1", a.LastName);
        }

        [Test]
        public void Count_With_Predicates_Returns_Correct_Count()
        {
            _connection.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            _connection.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            _connection.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
            _connection.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

            var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
            int count = _connection.Count<Person>(predicate);
            Assert.AreEqual(2, count);
        }
    }
}