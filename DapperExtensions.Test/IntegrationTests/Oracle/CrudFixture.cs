using DapperExtensions.Predicate;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Animal = DapperExtensions.Test.Data.Oracle.Animal;
using Multikey = DapperExtensions.Test.Data.Oracle.Multikey;
using Person = DapperExtensions.Test.Data.Oracle.Person;

namespace DapperExtensions.Test.IntegrationTests.Oracle
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public static class CrudFixture
    {
        [TestFixture]
        public class InsertMethod : OracleBaseFixture
        {
            [Test]
            public void AddsEntityToDatabase_ReturnsKey()
            {
                Person p = new Person { Active = "Y", FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                var id = Db.Insert(p);
                Assert.AreEqual(1, id);
                Assert.AreEqual(1, p.Id);
                Dispose();
            }

            [Test]
            public void AddsEntityToDatabase_ReturnsCompositeKey()
            {
                Multikey m = new Multikey { Key2 = "key", Value = "foo" };
                var key = Db.Insert(m);
                Assert.AreEqual(1, key.Key1);
                Assert.AreEqual("key", key.Key2);
                Dispose();
            }

            [Test]
            public void AddsEntityToDatabase_ReturnsGeneratedPrimaryKey()
            {
                Animal a1 = new Animal { Name = "Foo" };
                Db.Insert(a1);

                var a2 = Db.Get<Animal>(a1.Id);
                Assert.AreNotEqual(Guid.Empty, a2.Id);
                Assert.AreEqual(a1.Id, a2.Id);
                Dispose();
            }

            [Test]
            public void AddsMultipleEntitiesToDatabase()
            {
                Animal a1 = new Animal { Name = "Foo" };
                Animal a2 = new Animal { Name = "Bar" };
                Animal a3 = new Animal { Name = "Baz" };

                Db.Insert<Animal>(new[] { a1, a2, a3 });

                var animals = Db.GetList<Animal>().ToList();
                Assert.AreEqual(3, animals.Count);
                Dispose();
            }
        }

        [TestFixture]
        public class GetMethod : OracleBaseFixture
        {
            [Test]
            public void UsingKey_ReturnsEntity()
            {
                Person p1 = new Person
                {
                    Active = "Y",
                    FirstName = "Foo",
                    LastName = "Bar",
                    DateCreated = DateTime.UtcNow
                };
                var id = Db.Insert(p1);

                Person p2 = Db.Get<Person>(id);
                Assert.AreEqual(id, p2.Id);
                Assert.AreEqual("Foo", p2.FirstName);
                Assert.AreEqual("Bar", p2.LastName);
                Dispose();
            }

            [Test]
            public void UsingCompositeKey_ReturnsEntity()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1);

                Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
                Assert.AreEqual(1, m2.Key1);
                Assert.AreEqual("key", m2.Key2);
                Assert.AreEqual("bar", m2.Value);
                Dispose();
            }
        }

        [TestFixture]
        public class DeleteMethod : OracleBaseFixture
        {
            [Test]
            public void UsingKey_DeletesFromDatabase()
            {
                Person p1 = new Person
                {
                    Active = "Y",
                    FirstName = "Foo",
                    LastName = "Bar",
                    DateCreated = DateTime.UtcNow
                };
                var id = Db.Insert(p1);

                Person p2 = Db.Get<Person>(id);
                Db.Delete(p2);
                Assert.IsNull(Db.Get<Person>(id));
                Dispose();
            }

            [Test]
            //[Ignore] // TODO add MultiKey support for oracle
            public void UsingCompositeKey_DeletesFromDatabase()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1);

                Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
                Db.Delete(m2);
                Assert.IsNull(Db.Get<Multikey>(new { key.Key1, key.Key2 }));
                Dispose();
            }

            [Test]
            public void UsingPredicate_DeletesRows()
            {
                Person p1 = new Person { Active = "Y", FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p2 = new Person { Active = "Y", FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p3 = new Person { Active = "Y", FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
                Db.Insert(p1);
                Db.Insert(p2);
                Db.Insert(p3);

                var list = Db.GetList<Person>();
                Assert.AreEqual(3, list.Count());

                IPredicate pred = Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
                var result = Db.Delete<Person>(pred);
                Assert.IsTrue(result);

                list = Db.GetList<Person>();
                Assert.AreEqual(1, list.Count());
                Dispose();
            }

            [Test]
            public void UsingObject_DeletesRows()
            {
                Person p1 = new Person { Active = "Y", FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p2 = new Person { Active = "Y", FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p3 = new Person { Active = "Y", FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
                Db.Insert(p1);
                Db.Insert(p2);
                Db.Insert(p3);

                var list = Db.GetList<Person>();
                Assert.AreEqual(3, list.Count());

                var result = Db.Delete<Person>(new { LastName = "Bar" });
                Assert.IsTrue(result);

                list = Db.GetList<Person>();
                Assert.AreEqual(1, list.Count());
                Dispose();
            }
        }

        [TestFixture]
        public class UpdateMethod : OracleBaseFixture
        {
            [Test]
            public void UsingKey_UpdatesEntity()
            {
                Person p1 = new Person
                {
                    Active = "Y",
                    FirstName = "Foo",
                    LastName = "Bar",
                    DateCreated = DateTime.UtcNow
                };
                var id = Db.Insert(p1);

                var p2 = Db.Get<Person>(id);
                p2.FirstName = "Baz";
                p2.Active = "N";

                Db.Update(p2);

                var p3 = Db.Get<Person>(id);
                Assert.AreEqual("Baz", p3.FirstName);
                Assert.AreEqual("Bar", p3.LastName);
                Assert.AreEqual("N", p3.Active);
                Dispose();
            }

            [Test]
            //[Ignore] // TODO add MultiKey support for oracle
            public void UsingCompositeKey_UpdatesEntity()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1);

                Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
                m2.Key2 = "key";
                m2.Value = "barz";
                Db.Update(m2);

                Multikey m3 = Db.Get<Multikey>(new { Key1 = 1, Key2 = "key" });
                Assert.AreEqual(1, m3.Key1);
                Assert.AreEqual("key", m3.Key2);
                Assert.AreEqual("barz", m3.Value);
                Dispose();
            }
        }

        [TestFixture]
        public class GetListMethod : OracleBaseFixture
        {
            [Test]
            public void UsingNullPredicate_ReturnsAll()
            {
                Db.Insert(new Person { Active = "Y", FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "N", FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "Y", FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "N", FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

                IEnumerable<Person> list = Db.GetList<Person>();
                Assert.AreEqual(4, list.Count());
                Dispose();
            }

            [Test]
            public void UsingPredicate_ReturnsMatching()
            {
                Db.Insert(new Person { Active = "Y", FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "N", FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "Y", FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "N", FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, "Y");
                IEnumerable<Person> list = Db.GetList<Person>(predicate, null);
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
                Dispose();
            }

            [Test]
            public void UsingObject_ReturnsMatching()
            {
                Db.Insert(new Person { Active = "Y", FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "N", FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "Y", FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "N", FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

                var predicate = new { Active = "Y", FirstName = "c" };
                IEnumerable<Person> list = Db.GetList<Person>(predicate, null);
                Assert.AreEqual(1, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "c"));
                Dispose();
            }
        }

        [TestFixture]
        public class GetPageMethod : OracleBaseFixture
        {
            [Test]
            public void UsingNullPredicate_ReturnsMatching()
            {
                var id1 = Db.Insert(new Person { Active = "Y", FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id2 = Db.Insert(new Person { Active = "N", FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id3 = Db.Insert(new Person { Active = "Y", FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
                var id4 = Db.Insert(new Person { Active = "N", FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>("FirstName")
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(null, sort, 0, 2);
                Assert.AreEqual(2, list.Count());
                Assert.AreEqual(id2, list.First().Id);
                Assert.AreEqual(id1, list.Skip(1).First().Id);
                Dispose();
            }

            [Test]
            public void UsingPredicate_ReturnsMatching()
            {
                var id1 = Db.Insert(new Person { Active = "Y", FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id2 = Db.Insert(new Person { Active = "N", FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id3 = Db.Insert(new Person { Active = "Y", FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
                var id4 = Db.Insert(new Person { Active = "N", FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, "Y");
                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>("FirstName")
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(predicate, sort, 0, 2);
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
                Dispose();
            }

            [Test]
            public void NotFirstPage_Returns_NextResults()
            {
                var id1 = Db.Insert(new Person { Active = "Y", FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id2 = Db.Insert(new Person { Active = "N", FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id3 = Db.Insert(new Person { Active = "Y", FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
                var id4 = Db.Insert(new Person { Active = "N", FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>("FirstName")
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(null, sort, 2, 2);
                Assert.AreEqual(2, list.Count());
                Assert.AreEqual(id4, list.First().Id);
                Assert.AreEqual(id3, list.Skip(1).First().Id);
                Dispose();
            }

            [Test]
            public void UsingObject_ReturnsMatching()
            {
                var id1 = Db.Insert(new Person { Active = "Y", FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id2 = Db.Insert(new Person { Active = "N", FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id3 = Db.Insert(new Person { Active = "Y", FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
                var id4 = Db.Insert(new Person { Active = "N", FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

                var predicate = new { Active = "Y" };
                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>("FirstName")
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(predicate, sort, 0, 2);
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
                Dispose();
            }
        }

        [TestFixture]
        public class CountMethod : OracleBaseFixture
        {
            [Test]
            public void UsingNullPredicate_Returns_Count()
            {
                Db.Insert(new Person { Active = "Y", FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = "N", FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = "Y", FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = "N", FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

                int count = Db.Count<Person>(null);
                Assert.AreEqual(4, count);
                Dispose();
            }

            [Test]
            public void UsingPredicate_Returns_Count()
            {
                Db.Insert(new Person { Active = "Y", FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = "N", FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = "Y", FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = "N", FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

                var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
                int count = Db.Count<Person>(predicate);
                Assert.AreEqual(2, count);
                Dispose();
            }

            [Test]
            public void UsingObject_Returns_Count()
            {
                Db.Insert(new Person { Active = "Y", FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = "N", FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = "Y", FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = "N", FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

                var predicate = new { FirstName = new[] { "b", "d" } };
                int count = Db.Count<Person>(predicate);
                Assert.AreEqual(2, count);
                Dispose();
            }
        }

        [TestFixture]
        public class GetMultipleMethod : OracleBaseFixture
        {
            [Test]
            public void ReturnsItems()
            {
                Db.Insert(new Person { Active = "Y", FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = "N", FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = "Y", FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = "N", FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

                Db.Insert(new Animal { Name = "Foo" });
                Db.Insert(new Animal { Name = "Bar" });
                Db.Insert(new Animal { Name = "Baz" });

                var predicate = new GetMultiplePredicate();
                predicate.Add<Person>(null);
                predicate.Add<Animal>(Predicates.Field<Animal>(a => a.Name, Operator.Like, "Ba%"));
                predicate.Add<Person>(Predicates.Field<Person>(a => a.LastName, Operator.Eq, "c1"));

                var result = Db.GetMultiple(predicate);
                var people = result.Read<Person>().ToList();
                var animals = result.Read<Animal>().ToList();
                var people2 = result.Read<Person>().ToList();

                people.Should().HaveCount(4);
                animals.Should().HaveCount(2);
                people2.Should().HaveCount(1);
                Dispose();
            }
        }
    }
}