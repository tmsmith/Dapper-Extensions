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
    [TestFixture]
    public class PredicateDBFixture : BaseFixture
    {
        private SqlCeConnection _connection;
        private readonly string _databaseName;

        public PredicateDBFixture()
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

            cmd = new SqlCeCommand(ReadScriptFile("CreateFooTable"), _connection);
            cmd.ExecuteNonQuery();
        }

        public override void Teardown()
        {
            base.Teardown();
            _connection.Close();
        }

        [Test]
        public void Eq_Enumerable_Returns_Runs_Sql_Without_Error()
        {
            Person p1 = new Person { Active = true, FirstName = "Alpha", LastName = "Bar", DateCreated = DateTime.UtcNow };
            Person p2 = new Person { Active = true, FirstName = "Beta", LastName = "Bar", DateCreated = DateTime.UtcNow };
            Person p3 = new Person { Active = true, FirstName = "Gamma", LastName = "Bar", DateCreated = DateTime.UtcNow };
            _connection.Insert<Person>(new[] { p1, p2, p3 });

            var predicate = Predicates.Field<Person>(p => p.FirstName, Operator.Eq, new[] { "Alpha", "Gamma" });
            var result = _connection.GetList<Person>(predicate);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(r => r.FirstName == "Alpha"));
            Assert.IsTrue(result.Any(r => r.FirstName == "Gamma"));
        }

        [Test]
        public void Exists_Returns_Runs_Sql_Without_Error()
        {
            Person p1 = new Person { Active = true, FirstName = "Alpha", LastName = "Bar", DateCreated = DateTime.UtcNow };
            Person p2 = new Person { Active = true, FirstName = "Beta", LastName = "Bar", DateCreated = DateTime.UtcNow };
            Person p3 = new Person { Active = true, FirstName = "Gamma", LastName = "Bar", DateCreated = DateTime.UtcNow };
            _connection.Insert<Person>(new[] { p1, p2, p3 });

            Animal a1 = new Animal { Name = "Gamma" };
            Animal a2 = new Animal { Name = "Beta" };
            _connection.Insert<Animal>(new[] { a1, a2 });

            var subPredicate = Predicates.Property<Person, Animal>(p => p.FirstName, Operator.Eq, a => a.Name);
            var predicate = Predicates.Exists<Animal>(subPredicate);

            var result = _connection.GetList<Person>(predicate);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(r => r.FirstName == "Beta"));
            Assert.IsTrue(result.Any(r => r.FirstName == "Gamma"));
        }

        [Test]
        public void Test()
        {
            DapperExtensions.DefaultMapper = typeof(CustomMapper);
            Foo f = new Foo() { FirstName = "Foo", LastName = "Bar", DateOfBirth = DateTime.UtcNow.AddYears(-20) };
            _connection.Insert(f);
        }
    }
}