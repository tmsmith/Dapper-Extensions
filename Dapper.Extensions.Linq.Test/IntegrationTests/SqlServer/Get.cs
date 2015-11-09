using System;
using Dapper.Extensions.Linq.Test.Data;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class Get : SqlServerTests
    {
        [Test]
        public void UsingKey_ReturnsEntity()
        {
            Person p1 = new Person
            {
                Active = true,
                FirstName = "Foo",
                LastName = "Bar",
                DateCreated = DateTime.UtcNow
            };
            int id = Database.Insert(p1);

            Person p2 = Database.Get<Person>(id);
            Assert.AreEqual(id, p2.Id);
            Assert.AreEqual("Foo", p2.FirstName);
            Assert.AreEqual("Bar", p2.LastName);
        }

        [Test]
        public void UsingCompositeKey_ReturnsEntity()
        {
            Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
            var key = Database.Insert(m1);

            Multikey m2 = Database.Get<Multikey>(new { key.Key1, key.Key2 });
            Assert.AreEqual(1, m2.Key1);
            Assert.AreEqual("key", m2.Key2);
            Assert.AreEqual("bar", m2.Value);
        }
    }
}