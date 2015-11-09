using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Test.Data;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class GetList : SqlServerTests
    {
        [Test]
        public void UsingNullPredicate_ReturnsAll()
        {
            Database.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            Database.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            Database.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            Database.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

            IEnumerable<Person> list = Database.GetList<Person>();
            Assert.AreEqual(4, list.Count());
        }

        [Test]
        public void UsingPredicate_ReturnsMatching()
        {
            Database.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            Database.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            Database.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            Database.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

            var predicate = Predicates.Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
            IEnumerable<Person> list = Database.GetList<Person>(predicate, null);
            Assert.AreEqual(2, list.Count());
            Assert.IsTrue(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
        }

        [Test]
        public void UsingObject_ReturnsMatching()
        {
            Database.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
            Database.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
            Database.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
            Database.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

            var predicate = new { Active = true, FirstName = "c" };
            IEnumerable<Person> list = Database.GetList<Person>(predicate, null);
            Assert.AreEqual(1, list.Count());
            Assert.IsTrue(list.All(p => p.FirstName == "c"));
        }
    }
}