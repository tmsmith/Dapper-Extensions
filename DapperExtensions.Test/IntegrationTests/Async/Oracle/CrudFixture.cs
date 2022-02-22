using DapperExtensions.Predicate;
using DapperExtensions.Test.Data.Oracle;
using DapperExtensions.Test.IntegrationTests.Interfaces;
using FluentAssertions;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DapperExtensions.Test.IntegrationTests.Async.Oracle
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self)]
    public static class CrudFixture
    {
        [TestFixture]
        public class InsertMethod : OracleBaseAsyncFixture, IInsertMethod
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
                var a1 = new Foo { FirstName = "Foo" };
                Db.Insert(a1);

                var a2 = Db.Get<Foo>(a1.Id).Result;
                Assert.AreNotEqual(Guid.Empty, a2.Id);
                Assert.AreEqual(a1.Id, a2.Id);
                Dispose();
            }

            [Test]
            public void AddsMultipleEntitiesToDatabase()
            {
                var a1 = new Foo { FirstName = "Foo" };
                var a2 = new Foo { FirstName = "Bar" };
                var a3 = new Foo { FirstName = "Baz" };

                Db.Insert<Foo>(new[] { a1, a2, a3 });

                var animals = Db.GetList<Foo>().Result.ToList();
                Assert.AreEqual(3, animals.Count);
                Dispose();
            }

            [Test]
            [Ignore("GUID support for Oracle not implemented")]
            public void AddsEntityToDatabase_WithPassedInGuid()
            {
                throw new NotImplementedException();
            }

            [Test]
            [Ignore("GUID support for Oracle not implemented")]
            public void AddsMultipleEntitiesToDatabase_WithPassedInGuid()
            {
                throw new NotImplementedException();
            }
        }

        [TestFixture]
        public class GetMethod : OracleBaseAsyncFixture, IGetMethod
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

            [Test]
            public void UsingDirectConnection_ReturnsEntity()
            {
                var p = new Person { Active = "T", FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                long id = Db.Insert(p).Result;

                using (OracleConnection cn = new OracleConnection(ConnectionString))
                {
                    cn.Open();
                    var person = cn.GetAsync<Person>(id).Result;
                    cn.Close();

                    Assert.AreEqual(id, person.Id);
                    Assert.AreEqual("Foo", person.FirstName);
                    Assert.AreEqual("Bar", person.LastName);
                }
            }

            [Test]

            public void UsingKey_ReturnsEntityWithRelations()
            {
                var f1 = new Foo { FirstName = "First", LastName = "Last", DateOfBirth = DateTime.Now };
                long fooId = Db.Insert(f1).Result;

                var b1 = new Bar { FooId = fooId, Name = $"Bar1_For_{f1.FullName}" };
                Db.Insert(b1);

                var f2 = Db.Get<Foo>(fooId, includedReferences: new List<Type> { typeof(Bar) }).Result;

                Assert.AreEqual(fooId, f2.Id);
                Assert.AreEqual("First", f2.FirstName);
                Assert.AreEqual("Last", f2.LastName);
                Assert.AreEqual(1, f2.BarList.Count);
            }
        }

        [TestFixture]
        public class DeleteMethod : OracleBaseAsyncFixture, IDeleteMethod
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
                Task<Person> aux = Db.Get<Person>(id);

                aux.AsyncState.Should().BeNull();
                Dispose();
            }

            [Test]
            public void UsingCompositeKey_DeletesFromDatabase()
            {
                var m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1).Result;

                var m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 }).Result;
                var result = Db.Delete(m2).Result;
                Assert.IsTrue(result);
                var aux = Db.Get<Multikey>(new { key.Key1, key.Key2 });

                aux.AsyncState.Should().BeNull();
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
        public class UpdateMethod : OracleBaseAsyncFixture, IUpdateMethod
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

            [Test]
            [Ignore("GUID support for Oracle not implemented")]
            public void UsingGuidKey_UpdatesEntity()
            {
                throw new NotImplementedException();
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
                                        Predicates.Sort<Person>("FirstName")
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
                                        Predicates.Sort<Person>("FirstName")
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
                                        Predicates.Sort<Person>("FirstName")
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
                                        Predicates.Sort<Person>("FirstName")
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

                people.Should().HaveCount(4);
                animals.Should().HaveCount(2);
                people2.Should().HaveCount(1);
                Dispose();
            }
        }
    }
}