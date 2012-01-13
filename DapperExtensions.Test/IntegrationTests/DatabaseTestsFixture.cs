using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Test.Data;
using DapperExtensions.Test.Helpers;
using NUnit.Framework;

namespace DapperExtensions.Test.IntegrationTests
{
    [TestFixture]
    public class DatabaseTestsFixture
    {
        public class PredicateTests : DatabaseConnection
        {
            [Test]
            public void Eq_EnumerableType_GeneratesAndRunsProperSql()
            {
                Person p1 = new Person { Active = true, FirstName = "Alpha", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p2 = new Person { Active = true, FirstName = "Beta", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p3 = new Person { Active = true, FirstName = "Gamma", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Impl.Insert<Person>(Connection, new[] { p1, p2, p3 }, null, null);

                var predicate = Predicates.Field<Person>(p => p.FirstName, Operator.Eq, new[] { "Alpha", "Gamma" });
                var result = Impl.GetList<Person>(Connection, predicate, null, null, null, true);
                Assert.AreEqual(2, result.Count());
                Assert.IsTrue(result.Any(r => r.FirstName == "Alpha"));
                Assert.IsTrue(result.Any(r => r.FirstName == "Gamma"));
            }

            [Test]
            public void Exists_GeneratesAndRunsProperSql()
            {
                Person p1 = new Person { Active = true, FirstName = "Alpha", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p2 = new Person { Active = true, FirstName = "Beta", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p3 = new Person { Active = true, FirstName = "Gamma", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Impl.Insert<Person>(Connection, new[] { p1, p2, p3 }, null, null);

                Animal a1 = new Animal { Name = "Gamma" };
                Animal a2 = new Animal { Name = "Beta" };
                Impl.Insert<Animal>(Connection, new[] { a1, a2 }, null, null);

                var subPredicate = Predicates.Property<Person, Animal>(p => p.FirstName, Operator.Eq, a => a.Name);
                var predicate = Predicates.Exists<Animal>(subPredicate);

                var result = Impl.GetList<Person>(Connection, predicate, null, null, null, true);
                Assert.AreEqual(2, result.Count());
                Assert.IsTrue(result.Any(r => r.FirstName == "Beta"));
                Assert.IsTrue(result.Any(r => r.FirstName == "Gamma"));
            }

            [Test]
            public void Between_GeneratesAndRunsProperSql()
            {
                Person p1 = new Person { Active = true, FirstName = "Alpha", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p2 = new Person { Active = true, FirstName = "Beta", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p3 = new Person { Active = true, FirstName = "Gamma", LastName = "Omega", DateCreated = DateTime.UtcNow };
                Impl.Insert<Person>(Connection, new[] { p1, p2, p3 }, null, null);

                var pred = Predicates.Between<Person>(p => p.LastName,
                                                      new BetweenValues { Value1 = "Aaa", Value2 = "Bzz" });
                var result = Impl.GetList<Person>(Connection, pred, null, null, null, true).ToList();
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("Alpha", result[0].FirstName);
                Assert.AreEqual("Beta", result[1].FirstName);
            }
        }

        public class CustomMapperTests : DatabaseConnection
        {
            [Test]
            public void GeneratesAndRunsProperSql()
            {
                Impl = new DapperExtensions.DapperExtensionsImpl(typeof(CustomMapper), new DapperExtensions.SqlGeneratorImpl(new SqlCeDialect()));
                Foo f = new Foo { FirstName = "Foo", LastName = "Bar", DateOfBirth = DateTime.UtcNow.AddYears(-20) };
                Impl.Insert(Connection, f, null, null);
            }
        }
    }
}