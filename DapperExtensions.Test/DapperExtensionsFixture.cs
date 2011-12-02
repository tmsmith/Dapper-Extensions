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

            cmd = new SqlCeCommand(ReadScriptFile("CreateMultikeyTable"), _connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCeCommand(ReadScriptFile("CreateAnimalTable"), _connection);
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
        public void Insert_Multikey_Inserts_Entity()
        {
            Multikey m = new Multikey { Key2 = "key", Value = "foo" };
            var key = _connection.Insert(m);
            Assert.AreEqual(1, key.Key1);
            Assert.AreEqual("key", key.Key2);
        }

        [Test]
        public void Insert_With_Guid_Primary_Key_Generates_Guid_Before_Insert()
        {
            Animal a1 = new Animal { Name = "Foo" };
            _connection.Insert(a1);

            var a2 = _connection.Get<Animal>(a1.Id);
            Assert.AreNotEqual(Guid.Empty, a2.Id);
            Assert.AreEqual(a1.Id, a2.Id);
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
        public void Get_Multikey_Gets_Entity()
        {
            Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
            var key = _connection.Insert(m1);

            Multikey m2 = _connection.Get<Multikey>(new { key.Key1, key.Key2 });
            Assert.AreEqual(1, m2.Key1);
            Assert.AreEqual("key", m2.Key2);
            Assert.AreEqual("bar", m2.Value);
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
        public void Delete_Multikey_Deletes_Entity()
        {
            Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
            var key = _connection.Insert(m1);

            Multikey m2 = _connection.Get<Multikey>(new { key.Key1, key.Key2 });
            _connection.Delete(m2);
            Assert.IsNull(_connection.Get<Multikey>(new { key.Key1, key.Key2 }));
        }

        [Test]
        public void Delete_With_Pedicate_Deletes_Rows()
        {
            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            Person p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            Person p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
            _connection.Insert(p1);
            _connection.Insert(p2);
            _connection.Insert(p3);

            var list = _connection.GetList<Person>();
            Assert.AreEqual(3, list.Count());

            IPredicate pred = Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
            var result = _connection.Delete<Person>(pred);
            Assert.IsTrue(result);

            list = _connection.GetList<Person>();
            Assert.AreEqual(1, list.Count());
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
        public void Update_Multikey_Updates_Entity()
        {
            Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
            var key = _connection.Insert(m1);

            Multikey m2 = _connection.Get<Multikey>(new { key.Key1, key.Key2 });
            m2.Key2 = "key";
            m2.Value = "barz";
            _connection.Update(m2);

            Multikey m3 = _connection.Get<Multikey>(new { Key1 = 1, Key2 = "key" });
            Assert.AreEqual(1, m3.Key1);
            Assert.AreEqual("key", m3.Key2);
            Assert.AreEqual("barz", m3.Value);
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

        [Test]
        public void GetPage_With_Predicates_Returns_Correct_Items()
        {
            var id1 = _connection.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            var id2 = _connection.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            var id3 = _connection.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
            var id4 = _connection.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });
            IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

            IEnumerable<Person> list = _connection.GetPage<Person>(null, sort, 1, 2);
            Assert.AreEqual(2, list.Count());
            Assert.AreEqual(id2, list.First().Id);
            Assert.AreEqual(id1, list.Skip(1).First().Id);

            list = _connection.GetPage<Person>(null, sort, 2, 2);
            Assert.AreEqual(2, list.Count());
            Assert.AreEqual(id4, list.First().Id);
            Assert.AreEqual(id3, list.Skip(1).First().Id);

            list = _connection.GetPage<Person>(null, sort, 3, 2);
            Assert.AreEqual(0, list.Count());
        }

        [Test]
        public void GetNextGuid_Returns_Guid()
        {
            DapperExtensions.GetNextGuid();
        }
    }
}
