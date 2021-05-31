using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Animal = DapperExtensions.Test.Data.Common.Animal;
using Multikey = DapperExtensions.Test.Data.Sqlite.Multikey;
using Person = DapperExtensions.Test.Data.Common.Person;

namespace DapperExtensions.Test.IntegrationTests.Async.Sqlite
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public static class CrudFixture
    {
        [TestFixture]
        public class InsertMethod : SqliteBaseAsyncFixture
        {
            [Test]
            public void AddsEntityToDatabase_ReturnsKey()
            {
                var p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                var id = Db.Insert(p).Result;
                Assert.AreEqual(1, id);
                Assert.AreEqual(1, p.Id);
            }

            [Test]
            public void AddsEntityToDatabase_ReturnsCompositeKey()
            {
                var m = new Multikey { Key1 = 1, Key2 = "key", Value = "foo" };
                var key = Db.Insert(m).Result;
                Assert.AreEqual(1, key.Key1);
                Assert.AreEqual("key", key.Key2);
            }

            [Test]
            public void AddsEntityToDatabase_ReturnsGeneratedPrimaryKey()
            {
                var a1 = new Animal { Name = "Foo" };
                Db.Insert(a1);

                var a2 = Db.Get<Animal>(a1.Id).Result;
                Assert.AreNotEqual(Guid.Empty, a2.Id);
                Assert.AreEqual(a1.Id, a2.Id);
            }

            [Test]
            public void AddsEntityToDatabase_WithPassedInGuid()
            {
                var guid = Guid.NewGuid();
                var a1 = new Animal { Id = guid, Name = "Foo" };
                Db.Insert(a1);

                var a2 = Db.Get<Animal>(a1.Id).Result;
                Assert.AreNotEqual(Guid.Empty, a2.Id);
                Assert.AreEqual(guid, a2.Id);
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
            }

            [Test]
            public void AddsMultipleEntitiesToDatabase_WithPassedInGuid()
            {
                var guid1 = Guid.NewGuid();
                var a1 = new Animal { Id = guid1, Name = "Foo" };
                var guid2 = Guid.NewGuid();
                var a2 = new Animal { Id = guid2, Name = "Bar" };
                var guid3 = Guid.NewGuid();
                var a3 = new Animal { Id = guid3, Name = "Baz" };

                Db.Insert<Animal>(new[] { a1, a2, a3 });

                var animals = Db.GetList<Animal>().Result.ToList();
                Assert.AreEqual(3, animals.Count);
                Assert.IsNotNull(animals.Find(x => x.Id == guid1));
                Assert.IsNotNull(animals.Find(x => x.Id == guid2));
                Assert.IsNotNull(animals.Find(x => x.Id == guid3));
            }
        }

        [TestFixture]
        public class GetMethod : SqliteBaseAsyncFixture
        {
            [Test]
            public void UsingKey_ReturnsEntity()
            {
                var p1 = new Person
                {
                    Active = true,
                    FirstName = "Foo",
                    LastName = "Bar",
                    DateCreated = DateTime.UtcNow
                };
                var id = Db.Insert(p1).Result;

                var p2 = Db.Get<Person>(id).Result;
                Assert.AreEqual(id, p2.Id);
                Assert.AreEqual("Foo", p2.FirstName);
                Assert.AreEqual("Bar", p2.LastName);
            }

            [Test]
            public void UsingCompositeKey_ReturnsEntity()
            {
                var m1 = new Multikey { Key1 = 1, Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1).Result;

                var m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 }).Result;
                Assert.AreEqual(1, m2.Key1);
                Assert.AreEqual("key", m2.Key2);
                Assert.AreEqual("bar", m2.Value);
            }
        }

        [TestFixture]
        public class DeleteMethod : SqliteBaseAsyncFixture
        {
            private static void Arrange(out Person p1, out Person p2, out Person p3)
            {
                p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
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
            }

            [Test]
            public void UsingCompositeKey_DeletesFromDatabase()
            {
                var m1 = new Multikey { Key1 = 1, Key2 = "key2", Value = "bar" };
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
            }
        }

        [TestFixture]
        public class UpdateMethod : SqliteBaseAsyncFixture
        {
            [Test]
            public void UsingKey_UpdatesEntity()
            {
                var p1 = new Person
                {
                    Active = true,
                    FirstName = "Foo",
                    LastName = "Bar",
                    DateCreated = DateTime.UtcNow
                };
                var id = Db.Insert(p1).Result;

                var p2 = Db.Get<Person>(id).Result;
                p2.FirstName = "Baz";
                p2.Active = false;

                Db.Update(p2);

                var p3 = Db.Get<Person>(id).Result;
                Assert.AreEqual("Baz", p3.FirstName);
                Assert.AreEqual("Bar", p3.LastName);
                Assert.AreEqual(false, p3.Active);
            }

            [Test]
            public void UsingCompositeKey_UpdatesEntity()
            {
                var m1 = new Multikey { Key1 = 1, Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1).Result;

                var m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 }).Result;
                m2.Key2 = "key";
                m2.Value = "barz";
                Db.Update(m2);

                var m3 = Db.Get<Multikey>(new { Key1 = 1, Key2 = "key" }).Result;
                Assert.AreEqual(1, m3.Key1);
                Assert.AreEqual("key", m3.Key2);
                Assert.AreEqual("barz", m3.Value);
            }
        }

        [TestFixture]
        public class GetListMethod : SqliteBaseAsyncFixture
        {
            private void Arrange()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });
            }

            [Test]
            public void UsingNullPredicate_ReturnsAll()
            {
                Arrange();

                var list = Db.GetList<Person>().Result;
                Assert.AreEqual(4, list.Count());
            }

            [Test]
            public void UsingPredicate_ReturnsMatching()
            {
                Arrange();

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
                var list = Db.GetList<Person>(predicate, null).Result;
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
            }

            [Test]
            public void UsingObject_ReturnsMatching()
            {
                Arrange();

                var predicate = new { Active = true, FirstName = "c" };
                var list = Db.GetList<Person>(predicate, null).Result;
                Assert.AreEqual(1, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "c"));
            }

            //[Test]
            //public void UsingProjections_Returns_ChosenField()
            //{
            //    Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });

            //    var projections = new List<IProjection>() { Predicates.Projection<Person>(x => x.FirstName) };

            //    var results = Db.GetList<Person>(colsToSelect: projections);

            //    Assert.AreEqual(results.Count(), 1);

            //    var result = results.Single();

            //    Assert.AreEqual(result.FirstName, "a");
            //    Assert.IsNull(result.LastName);
            //    Assert.IsFalse(result.Active);
            //    Assert.AreEqual(result.DateCreated, default(DateTime));
            //}
        }

        [TestFixture]
        public class GetPageMethod : SqliteBaseAsyncFixture
        {
            private void Arrange(out dynamic id1, out dynamic id2, out dynamic id3, out dynamic id4)
            {
                id1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow }).Result;
                id2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow }).Result;
                id3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow }).Result;
                id4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow }).Result;
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
            }

            [Test]
            public void UsingPredicate_ReturnsMatching()
            {
                Arrange(out var id1, out var id2, out var id3, out var id4);

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
                var sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                var list = Db.GetPage<Person>(predicate, sort, 0, 2).Result;
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
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
            }

            [Test]
            public void UsingObject_ReturnsMatching()
            {
                Arrange(out var id1, out var id2, out var id3, out var id4);

                var predicate = new { Active = true };
                var sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                var list = Db.GetPage<Person>(predicate, sort, 0, 2).Result;
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
            }
        }

        [TestFixture]
        public class CountMethod : SqliteBaseAsyncFixture
        {
            private void Arrange()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });
            }

            [Test]
            public void UsingNullPredicate_Returns_Count()
            {
                Arrange();

                var count = Db.Count<Person>(null).Result;
                Assert.AreEqual(4, count);
            }

            [Test]
            public void UsingPredicate_Returns_Count()
            {
                Arrange();

                var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
                var count = Db.Count<Person>(predicate).Result;
                Assert.AreEqual(2, count);
            }

            [Test]
            public void UsingObject_Returns_Count()
            {
                Arrange();

                var predicate = new { FirstName = new[] { "b", "d" } };
                var count = Db.Count<Person>(predicate).Result;
                Assert.AreEqual(2, count);
            }
        }

        [TestFixture]
        public class GetMultipleMethod : SqliteBaseAsyncFixture
        {
            [Test]
            public void ReturnsItems()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

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
            }
        }
    }
}