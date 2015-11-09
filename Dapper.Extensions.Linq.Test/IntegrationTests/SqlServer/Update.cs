using System;
using Dapper.Extensions.Linq.Test.Data;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class Update : SqlServerTests
    {
        [Test]
        public void UsingKey_UpdatesEntity()
        {
            Person p1 = new Person
            {
                Active = true,
                FirstName = "Foo",
                LastName = "Bar",
                DateCreated = DateTime.UtcNow
            };
            int id = Database.Insert(p1);

            var p2 = Database.Get<Person>(id);
            p2.FirstName = "Baz";
            p2.Active = false;

            Database.Update(p2);

            var p3 = Database.Get<Person>(id);
            Assert.AreEqual("Baz", p3.FirstName);
            Assert.AreEqual("Bar", p3.LastName);
            Assert.AreEqual(false, p3.Active);
        }

        [Test]
        public void UsingCompositeKey_UpdatesEntity()
        {
            Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
            var key = Database.Insert(m1);

            Multikey m2 = Database.Get<Multikey>(new { key.Key1, key.Key2 });
            m2.Key2 = "key";
            m2.Value = "barz";
            Database.Update(m2);

            Multikey m3 = Database.Get<Multikey>(new { Key1 = 1, Key2 = "key" });
            Assert.AreEqual(1, m3.Key1);
            Assert.AreEqual("key", m3.Key2);
            Assert.AreEqual("barz", m3.Value);
        }
    }
}
