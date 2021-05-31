using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Animal = DapperExtensions.Test.Data.Oracle.Animal;
using Multikey = DapperExtensions.Test.Data.Oracle.Multikey;
using Person = DapperExtensions.Test.Data.Oracle.Person;

namespace DapperExtensions.Test.IntegrationTests.Async.Oracle
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public static class CrudFixture
    {
        [TestFixture]
        public class InsertMethod : OracleBaseAsyncFixture
        {
            [Test]
            public void AddsEntityToDatabase_ReturnsKey()
            {
                var p = new Person { Active = "Y", FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                var id = Db.Insert(p).Result;
                Assert.AreEqual(1, id);
                Assert.AreEqual(1, p.Id);
                Dispose();
            }

            [Test]
            public void AddsEntityToDatabase_ReturnsCompositeKey()
            {
                var m = new Multikey { Key2 = "key", Value = "foo" };
                var key = Db.Insert(m).Result;
                Assert.AreEqual(1, key.Key1);
                Assert.AreEqual("key", key.Key2);
                Dispose();
            }

            [Test]
            public void AddsEntityToDatabase_ReturnsGeneratedPrimaryKey()
            {
                var a1 = new Animal { Name = "Foo" };
                Db.Insert(a1);

                var a2 = Db.Get<Animal>(a1.Id).Result;
                Assert.AreNotEqual(Guid.Empty, a2.Id);
                Assert.AreEqual(a1.Id, a2.Id);
                Dispose();
            }

            [Test]
            public void AddsMultipleEntitiesToDatabase()
            {
                var a1 = new Animal { Name = "Foo" };
                var a2 = new Animal { Name = "Bar" };
                var a3 = new Animal { Name = "Baz" };

                Db.Insert<Animal>(new[] { a1, a2, a3 });

                var animals = Db.GetList<Animal>().Result.ToList();
                Assert.AreEqual(3, animals.Count);
                Dispose();
            }
        }

        [TestFixture]
        public class GetMethod : OracleBaseAsyncFixture
        {
            [Test]
            public void UsingKey_ReturnsEntity()
            {
                var p1 = new Person
                {
                    Active = "Y",
                    FirstName = "Foo",
                    LastName = "Bar",
                    DateCreated = DateTime.UtcNow
                };
                var id = Db.Insert(p1).Result;

                var p2 = Db.Get<Person>(id).Result;
                Assert.AreEqual(id, p2.Id);
                Assert.AreEqual("Foo", p2.FirstName);
                Assert.AreEqual("Bar", p2.LastName);
                Dispose();
            }

            [Test]
            public void UsingCompositeKey_ReturnsEntity()
            {
                var m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1).Result;

                var m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 }).Result;
                Assert.AreEqual(1, m2.Key1);
                Assert.AreEqual("key", m2.Key2);
                Assert.AreEqual("bar", m2.Value);
                Dispose();
            }
        }

        [TestFixture]
        public class DeleteMethod : OracleBaseAsyncFixture
        {
            private static void Arrange(out Person p1, out Person p2, out Person p3)
            {
                p1 = new Person { Active = "Y", FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                p2 = new Person { Active = "Y", FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                p3 = new Person { Active = "Y", FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
            }

            [Test]
            public void UsingKey_DeletesFromDatabase()
            {
                Arrange(out var p1, out var _, out var _);
                var id = Db.Insert(p1).Result;

                var p2 = Db.Get<Person>(id).Result;
                Assert.IsTrue(Db.Delete(p2).Result);
                var aux = Db.Get<Person>(id);

                if (aux.AsyncState == null)
                {
                    Assert.IsNull(aux.AsyncState);
                }
                else
                {
                    Assert.IsNull(aux.Result);
                }
                Dispose();
            }

            [Test]
            public void UsingCompositeKey_DeletesFromDatabase()
            {
                var m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1).Result;

                var m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 }).Result;
                Assert.IsTrue(Db.Delete(m2).Result);
                var aux = Db.Get<Multikey>(new { key.Key1, key.Key2 });

                if (aux.AsyncState == null)
                {
                    Assert.IsNull(aux.AsyncState);
                }
                else
                {
                    Assert.IsNull(aux.Result);
                }
                Dispose();
            }

            [Test]
            public void UsingPredicate_DeletesRows()
            {
                Arrange(out var p1, out var p2, out var p3);

                Db.Insert(p1);
                Db.Insert(p2);
                Db.Insert(p3);

                var list = Db.GetList<Person>().Result;
                Assert.AreEqual(3, list.Count());

                var pred = Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
                var result = Db.Delete<Person>(pred).Result;
                Assert.IsTrue(result);

                list = Db.GetList<Person>().Result;
                Assert.AreEqual(1, list.Count());
                Dispose();
            }

            [Test]
            public void UsingObject_DeletesRows()
            {
                Arrange(out var p1, out var p2, out var p3);

                Db.Insert(p1);
                Db.Insert(p2);
                Db.Insert(p3);

                var list = Db.GetList<Person>().Result;
                Assert.AreEqual(3, list.Count());

                var result = Db.Delete<Person>(new { LastName = "Bar" }).Result;
                Assert.IsTrue(result);

                list = Db.GetList<Person>().Result;
                Assert.AreEqual(1, list.Count());
                Dispose();
            }
        }

        [TestFixture]
        public class UpdateMethod : OracleBaseAsyncFixture
        {
            [Test]
            public void UsingKey_UpdatesEntity()
            {
                var p1 = new Person
                {
                    Active = "Y",
                    FirstName = "Foo",
                    LastName = "Bar",
                    DateCreated = DateTime.UtcNow
                };
                var id = Db.Insert(p1).Result;

                var p2 = Db.Get<Person>(id).Result;
                p2.FirstName = "Baz";
                p2.Active = "N";

                Db.Update(p2);

                var p3 = Db.Get<Person>(id).Result;
                Assert.AreEqual("Baz", p3.FirstName);
                Assert.AreEqual("Bar", p3.LastName);
                Assert.AreEqual("N", p3.Active);
                Dispose();
            }

            [Test]
            public void UsingCompositeKey_UpdatesEntity()
            {
                var m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1).Result;

                var m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 }).Result;
                m2.Key2 = "key";
                m2.Value = "barz";
                Db.Update(m2);

                var m3 = Db.Get<Multikey>(new { Key1 = 1, Key2 = "key" }).Result;
                Assert.AreEqual(1, m3.Key1);
                Assert.AreEqual("key", m3.Key2);
                Assert.AreEqual("barz", m3.Value);
                Dispose();
            }
        }

        [TestFixture]
        public class GetListMethod : OracleBaseAsyncFixture
        {
            private void Arrange()
            {
                Db.Insert(new Person { Active = "Y", FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "N", FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "Y", FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = "N", FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });
            }

            [Test]
            public void UsingNullPredicate_ReturnsAll()
            {
                Arrange();

                var list = Db.GetList<Person>().Result;
                Assert.AreEqual(4, list.Count());
                Dispose();
            }

            [Test]
            public void UsingPredicate_ReturnsMatching()
            {
                Arrange();

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, "Y");
                var list = Db.GetList<Person>(predicate, null).Result;
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
                Dispose();
            }

            [Test]
            public void UsingObject_ReturnsMatching()
            {
                Arrange();

                var predicate = new { Active = "Y", FirstName = "c" };
                var list = Db.GetList<Person>(predicate, null).Result;
                Assert.AreEqual(1, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "c"));
                Dispose();
            }
        }

        [TestFixture]
        public class GetPageMethod : OracleBaseAsyncFixture
        {
            private void Arrange(out dynamic id1, out dynamic id2, out dynamic id3, out dynamic id4)
            {
                id1 = Db.Insert(new Person { Active = "Y", FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow }).Result;
                id2 = Db.Insert(new Person { Active = "N", FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow }).Result;
                id3 = Db.Insert(new Person { Active = "Y", FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow }).Result;
                id4 = Db.Insert(new Person { Active = "N", FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow }).Result;
            }

            [Test]
            public void UsingNullPredicate_ReturnsMatching()
            {
                Arrange(out var id1, out var id2, out var id3, out var id4);

                var sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                var list = Db.GetPage<Person>(null, sort, 0, 2).Result;
                Assert.AreEqual(2, list.Count());
                Assert.AreEqual(id2, list.First().Id);
                Assert.AreEqual(id1, list.Skip(1).First().Id);
                Dispose();
            }

            [Test]
            public void UsingPredicate_ReturnsMatching()
            {
                Arrange(out var id1, out var id2, out var id3, out var id4);

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, "Y");
                var sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                var list = Db.GetPage<Person>(predicate, sort, 0, 2).Result;
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
                Dispose();
            }

            [Test]
            public void NotFirstPage_Returns_NextResults()
            {
                Arrange(out var id1, out var id2, out var id3, out var id4);

                var sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                var list = Db.GetPage<Person>(null, sort, 2, 2).Result;
                Assert.AreEqual(2, list.Count());
                Assert.AreEqual(id4, list.First().Id);
                Assert.AreEqual(id3, list.Skip(1).First().Id);
                Dispose();
            }

            [Test]
            public void UsingObject_ReturnsMatching()
            {
                Arrange(out var id1, out var id2, out var id3, out var id4);

                var predicate = new { Active = "Y" };
                var sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                var list = Db.GetPage<Person>(predicate, sort, 0, 2).Result;
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
                Dispose();
            }
        }

        [TestFixture]
        public class CountMethod : OracleBaseAsyncFixture
        {
            private void Arrange()
            {
                Db.Insert(new Person { Active = "Y", FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = "N", FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = "Y", FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = "N", FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });
            }

            [Test]
            public void UsingNullPredicate_Returns_Count()
            {
                Arrange();

                var count = Db.Count<Person>(null).Result;
                Assert.AreEqual(4, count);
                Dispose();
            }

            [Test]
            public void UsingPredicate_Returns_Count()
            {
                Arrange();

                var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
                var count = Db.Count<Person>(predicate).Result;
                Assert.AreEqual(2, count);
                Dispose();
            }

            [Test]
            public void UsingObject_Returns_Count()
            {
                Arrange();

                var predicate = new { FirstName = new[] { "b", "d" } };
                var count = Db.Count<Person>(predicate).Result;
                Assert.AreEqual(2, count);
                Dispose();
            }
        }

        [TestFixture]
        public class GetMultipleMethod : OracleBaseAsyncFixture
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

                var result = Db.GetMultiple(predicate).Result;
                var people = result.Read<Person>().ToList();
                var animals = result.Read<Animal>().ToList();
                var people2 = result.Read<Person>().ToList();

                Assert.AreEqual(4, people.Count);
                Assert.AreEqual(2, animals.Count);
                Assert.AreEqual(1, people2.Count);
                Dispose();
            }
        }
    }
}