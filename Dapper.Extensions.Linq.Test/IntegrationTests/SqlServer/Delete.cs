using System;
using System.Linq;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Test.Data;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class Delete : SqlServerTests
    {
        [Test]
        public void UsingKey_DeletesFrom_database()
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
            Database.Delete(p2);
            Assert.IsNull(Database.Get<Person>(id));
        }

        [Test]
        public void UsingCompositeKey_DeletesFrom_database()
        {
            Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
            var key = Database.Insert(m1);

            Multikey m2 = Database.Get<Multikey>(new { key.Key1, key.Key2 });
            Database.Delete(m2);
            Assert.IsNull(Database.Get<Multikey>(new { key.Key1, key.Key2 }));
        }

        [Test]
        public void UsingPredicate_DeletesRows()
        {
            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            Person p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            Person p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
            Database.Insert(p1);
            Database.Insert(p2);
            Database.Insert(p3);

            var list = Database.GetList<Person>();
            Assert.AreEqual(3, list.Count());

            IPredicate pred = Predicates.Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
            var result = Database.Delete<Person>(pred);
            Assert.IsTrue(result);

            list = Database.GetList<Person>();
            Assert.AreEqual(1, list.Count());
        }

        [Test]
        public void UsingObject_DeletesRows()
        {
            Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            Person p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
            Person p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
            Database.Insert(p1);
            Database.Insert(p2);
            Database.Insert(p3);

            var list = Database.GetList<Person>();
            Assert.AreEqual(3, list.Count());

            var result = Database.Delete<Person>(new { LastName = "Bar" });
            Assert.IsTrue(result);

            list = Database.GetList<Person>();
            Assert.AreEqual(1, list.Count());
        }
    }
}