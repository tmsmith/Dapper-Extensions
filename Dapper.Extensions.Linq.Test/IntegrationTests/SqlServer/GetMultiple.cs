using System;
using System.Linq;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Predicates;
using Dapper.Extensions.Linq.Test.Data;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class GetMultiple : SqlServerTests
    {
        [Test]
        public void ReturnsItems()
        {
            Database.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            Database.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
            Database.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
            Database.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

            Database.Insert(new Animal { Name = "Foo" });
            Database.Insert(new Animal { Name = "Bar" });
            Database.Insert(new Animal { Name = "Baz" });

            GetMultiplePredicate predicate = new GetMultiplePredicate();
            predicate.Add<Person>(null);
            predicate.Add<Animal>(Predicates.Predicates.Field<Animal>(a => a.Name, Operator.Like, "Ba%"));
            predicate.Add<Person>(Predicates.Predicates.Field<Person>(a => a.LastName, Operator.Eq, "c1"));

            var result = Database.GetMultiple(predicate);
            var people = result.Read<Person>().ToList();
            var animals = result.Read<Animal>().ToList();
            var people2 = result.Read<Person>().ToList();

            Assert.AreEqual(4, people.Count);
            Assert.AreEqual(2, animals.Count);
            Assert.AreEqual(1, people2.Count);
        }
    }
}