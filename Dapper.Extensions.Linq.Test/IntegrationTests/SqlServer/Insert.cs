using System;
using System.Linq;
using Dapper.Extensions.Linq.Test.Data;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class Insert : SqlServerTests
    {
        [Test]
        public void AddsEntityTo_database_ReturnsKey()
        {
            Person p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            int id = Database.Insert(p);
            Assert.AreEqual(1, id);
            Assert.AreEqual(1, p.Id);
        }

        [Test]
        public void AddsEntityTo_database_ReturnsCompositeKey()
        {
            Multikey m = new Multikey { Key2 = "key", Value = "foo" };
            var key = Database.Insert(m);
            Assert.AreEqual(1, key.Key1);
            Assert.AreEqual("key", key.Key2);
        }

        [Test]
        public void AddsEntityTo_database_ReturnsGeneratedPrimaryKey()
        {
            Animal a1 = new Animal { Name = "Foo" };
            Database.Insert(a1);

            var a2 = Database.Get<Animal>(a1.Id);
            Assert.AreNotEqual(Guid.Empty, a2.Id);
            Assert.AreEqual(a1.Id, a2.Id);
        }

        [Test]
        public void AddsMultipleEntitiesTo_database()
        {
            Animal a1 = new Animal { Name = "Foo" };
            Animal a2 = new Animal { Name = "Bar" };
            Animal a3 = new Animal { Name = "Baz" };

            Database.Insert<Animal>(new[] { a1, a2, a3 });

            var animals = Database.GetList<Animal>().ToList();
            Assert.AreEqual(3, animals.Count);
        }
    }
}