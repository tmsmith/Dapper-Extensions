using System;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Test.Data;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class Count : SqlServerTests
    {
        [Test]
        public void UsingNullPredicate_Returns_Count()
        {
            Database.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            Database.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            Database.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
            Database.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

            int count = Database.Count<Person>(null);
            Assert.AreEqual(4, count);
        }

        [Test]
        public void UsingPredicate_Returns_Count()
        {
            Database.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            Database.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            Database.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
            Database.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

            var predicate = Predicates.Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
            int count = Database.Count<Person>(predicate);
            Assert.AreEqual(2, count);
        }

        [Test]
        public void UsingObject_Returns_Count()
        {
            Database.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            Database.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            Database.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
            Database.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

            var predicate = new { FirstName = new[] { "b", "d" } };
            int count = Database.Count<Person>(predicate);
            Assert.AreEqual(2, count);
        }
    }
}