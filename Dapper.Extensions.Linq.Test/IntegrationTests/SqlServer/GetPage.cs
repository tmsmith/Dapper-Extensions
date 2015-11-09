using System;
using System.Linq;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Test.Data;
using NUnit.Framework;
using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.SqlServer
{
    public class GetPage : SqlServerTests
    {
        [Test]
        public void UsingNullPredicate_ReturnsMatching()
        {
            var id1 = Database.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            var id2 = Database.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            var id3 = Database.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
            var id4 = Database.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

            IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Predicates.Sort<Person>(p => p.FirstName)
                                    };

            IEnumerable<Person> list = Database.GetPage<Person>(null, sort, 0, 2);
            Assert.AreEqual(2, list.Count());
            Assert.AreEqual(id2, list.First().Id);
            Assert.AreEqual(id1, list.Skip(1).First().Id);
        }

        [Test]
        public void UsingPredicate_ReturnsMatching()
        {
            var id1 = Database.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            var id2 = Database.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            var id3 = Database.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
            var id4 = Database.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

            var predicate = Predicates.Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
            IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Predicates.Sort<Person>(p => p.FirstName)
                                    };

            IEnumerable<Person> list = Database.GetPage<Person>(predicate, sort, 0, 3);
            Assert.AreEqual(2, list.Count());
            Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
        }

        [Test]
        public void NotFirstPage_Returns_NextResults()
        {
            var id1 = Database.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            var id2 = Database.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            var id3 = Database.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
            var id4 = Database.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

            IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Predicates.Sort<Person>(p => p.FirstName)
                                    };

            IEnumerable<Person> list = Database.GetPage<Person>(null, sort, 1, 2);
            Assert.AreEqual(2, list.Count());
            Assert.AreEqual(id4, list.First().Id);
            Assert.AreEqual(id3, list.Skip(1).First().Id);
        }

        [Test]
        public void UsingObject_ReturnsMatching()
        {
            var id1 = Database.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            var id2 = Database.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
            var id3 = Database.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
            var id4 = Database.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

            var predicate = new { Active = true };
            IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Predicates.Sort<Person>(p => p.FirstName)
                                    };

            IEnumerable<Person> list = Database.GetPage<Person>(predicate, sort, 0, 3);
            Assert.AreEqual(2, list.Count());
            Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
        }
    }
}