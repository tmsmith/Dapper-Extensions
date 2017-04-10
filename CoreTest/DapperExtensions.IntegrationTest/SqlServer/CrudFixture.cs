using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Test.Data;
using Xunit;

namespace DapperExtensions.IntegrationTests.SqlServer
{
    public class CrudFixture
    {      
        public class InsertMethod : SqlServerBaseFixture
        {
            [Fact]
            public void AddsEntityToDatabase_ReturnsKey()
            {
                Person p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                int id = Db.Insert(p);
                Assert.Equal(1, id);
                Assert.Equal(1, p.Id);
            }

            [Fact]
            public void AddsEntityToDatabase_ReturnsCompositeKey()
            {
                Multikey m = new Multikey { Key2 = "key", Value = "foo" };
                var key = Db.Insert(m);
                Assert.Equal(1, key.Key1);
                Assert.Equal("key", key.Key2);
            }

            [Fact]
            public void AddsEntityToDatabase_ReturnsGeneratedPrimaryKey()
            {
                Animal a1 = new Animal { Name = "Foo" };
                Db.Insert(a1);

                var a2 = Db.Get<Animal>(a1.Id);
                Assert.NotEqual(Guid.Empty, a2.Id);
                Assert.Equal(a1.Id, a2.Id);
            }

            [Fact]
            public void AddsMultipleEntitiesToDatabase()
            {
                Animal a1 = new Animal { Name = "Foo" };
                Animal a2 = new Animal { Name = "Bar" };
                Animal a3 = new Animal { Name = "Baz" };

                Db.Insert<Animal>(new[] { a1, a2, a3 });

                var animals = Db.GetList<Animal>().ToList();
                Assert.Equal(3, animals.Count);
            }
        }

        
        public class GetMethod : SqlServerBaseFixture
        {
            [Fact]
            public void UsingKey_ReturnsEntity()
            {
                Person p1 = new Person
                                {
                                    Active = true,
                                    FirstName = "Foo",
                                    LastName = "Bar",
                                    DateCreated = DateTime.UtcNow
                                };
                int id = Db.Insert(p1);

                Person p2 = Db.Get<Person>(id);
                Assert.Equal(id, p2.Id);
                Assert.Equal("Foo", p2.FirstName);
                Assert.Equal("Bar", p2.LastName);
            }

            [Fact]
            public void UsingCompositeKey_ReturnsEntity()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1);

                Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
                Assert.Equal(1, m2.Key1);
                Assert.Equal("key", m2.Key2);
                Assert.Equal("bar", m2.Value);
            }
        }

        
        public class DeleteMethod : SqlServerBaseFixture
        {
            [Fact]
            public void UsingKey_DeletesFromDatabase()
            {
                Person p1 = new Person
                                {
                                    Active = true,
                                    FirstName = "Foo",
                                    LastName = "Bar",
                                    DateCreated = DateTime.UtcNow
                                };
                int id = Db.Insert(p1);

                Person p2 = Db.Get<Person>(id);
                Db.Delete(p2);
                Assert.Null(Db.Get<Person>(id));
            }

            [Fact]
            public void UsingCompositeKey_DeletesFromDatabase()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1);

                Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
                Db.Delete(m2);
                Assert.Null(Db.Get<Multikey>(new { key.Key1, key.Key2 }));
            }

            [Fact]
            public void UsingPredicate_DeletesRows()
            {
                Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
                Db.Insert(p1);
                Db.Insert(p2);
                Db.Insert(p3);

                var list = Db.GetList<Person>();
                Assert.Equal(3, list.Count());

                IPredicate pred = Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
                var result = Db.Delete<Person>(pred);
                Assert.True(result);

                list = Db.GetList<Person>();
                Assert.Equal(1, list.Count());
            }

            [Fact]
            public void UsingObject_DeletesRows()
            {
                Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
                Db.Insert(p1);
                Db.Insert(p2);
                Db.Insert(p3);

                var list = Db.GetList<Person>();
                Assert.Equal(3, list.Count());

                var result = Db.Delete<Person>(new { LastName = "Bar"} );
                Assert.True(result);

                list = Db.GetList<Person>();
                Assert.Equal(1, list.Count());
            }
        }

        
        public class UpdateMethod : SqlServerBaseFixture
        {
            [Fact]
            public void UsingKey_UpdatesEntity()
            {
                Person p1 = new Person
                                {
                                    Active = true,
                                    FirstName = "Foo",
                                    LastName = "Bar",
                                    DateCreated = DateTime.UtcNow
                                };
                int id = Db.Insert(p1);

                var p2 = Db.Get<Person>(id);
                p2.FirstName = "Baz";
                p2.Active = false;

                Db.Update(p2);

                var p3 = Db.Get<Person>(id);
                Assert.Equal("Baz", p3.FirstName);
                Assert.Equal("Bar", p3.LastName);
                Assert.Equal(false, p3.Active);
            }

            [Fact]
            public void UsingCompositeKey_UpdatesEntity()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Db.Insert(m1);

                Multikey m2 = Db.Get<Multikey>(new { key.Key1, key.Key2 });
                m2.Key2 = "key";
                m2.Value = "barz";
                Db.Update(m2);

                Multikey m3 = Db.Get<Multikey>(new { Key1 = 1, Key2 = "key" });
                Assert.Equal(1, m3.Key1);
                Assert.Equal("key", m3.Key2);
                Assert.Equal("barz", m3.Value);
            }
        }

        
        public class GetListMethod : SqlServerBaseFixture
        {
            [Fact]
            public void UsingNullPredicate_ReturnsAll()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

                IEnumerable<Person> list = Db.GetList<Person>();
                Assert.Equal(4, list.Count());
            }

            [Fact]
            public void UsingPredicate_ReturnsMatching()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
                IEnumerable<Person> list = Db.GetList<Person>(predicate, null);
                Assert.Equal(2, list.Count());
                Assert.True(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
            }

            [Fact]
            public void UsingObject_ReturnsMatching()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow });

                var predicate = new { Active = true, FirstName = "c" };
                IEnumerable<Person> list = Db.GetList<Person>(predicate, null);
                Assert.Equal(1, list.Count());
                Assert.True(list.All(p => p.FirstName == "c"));
            }
        }

        
        public class GetPageMethod : SqlServerBaseFixture
        {
            [Fact]
            public void UsingNullPredicate_ReturnsMatching()
            {
                var id1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
                var id4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(null, sort, 0, 2);
                Assert.Equal(2, list.Count());
                Assert.Equal(id2, list.First().Id);
                Assert.Equal(id1, list.Skip(1).First().Id);
            }

            [Fact]
            public void UsingPredicate_ReturnsMatching()
            {
                var id1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
                var id4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(predicate, sort, 0, 3);
                Assert.Equal(2, list.Count());
                Assert.True(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
            }

            [Fact]
            public void NotFirstPage_Returns_NextResults()
            {
                var id1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
                var id4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(null, sort, 1, 2);
                Assert.Equal(2, list.Count());
                Assert.Equal(id4, list.First().Id);
                Assert.Equal(id3, list.Skip(1).First().Id);
            }

            [Fact]
            public void UsingObject_ReturnsMatching()
            {
                var id1 = Db.Insert(new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id2 = Db.Insert(new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow });
                var id3 = Db.Insert(new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow });
                var id4 = Db.Insert(new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow });

                var predicate = new { Active = true };
                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                IEnumerable<Person> list = Db.GetPage<Person>(predicate, sort, 0, 3);
                Assert.Equal(2, list.Count());
                Assert.True(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
            }
        }

        
        public class CountMethod : SqlServerBaseFixture
        {
            [Fact]
            public void UsingNullPredicate_Returns_Count()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

                int count = Db.Count<Person>(null);
                Assert.Equal(4, count);
            }

            [Fact]
            public void UsingPredicate_Returns_Count()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

                var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
                int count = Db.Count<Person>(predicate);
                Assert.Equal(2, count);
            }

            [Fact]
            public void UsingObject_Returns_Count()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

                var predicate = new { FirstName = new[] { "b", "d" } };
                int count = Db.Count<Person>(predicate);
                Assert.Equal(2, count);
            }
        }

        
        public class GetMultipleMethod : SqlServerBaseFixture
        {
            [Fact]
            public void ReturnsItems()
            {
                Db.Insert(new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) });
                Db.Insert(new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) });
                Db.Insert(new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) });

                Db.Insert(new Animal { Name = "Foo" });
                Db.Insert(new Animal { Name = "Bar" });
                Db.Insert(new Animal { Name = "Baz" });

                GetMultiplePredicate predicate = new GetMultiplePredicate();
                predicate.Add<Person>(null);
                predicate.Add<Animal>(Predicates.Field<Animal>(a => a.Name, Operator.Like, "Ba%"));
                predicate.Add<Person>(Predicates.Field<Person>(a => a.LastName, Operator.Eq, "c1"));

                var result = Db.GetMultiple(predicate);
                var people = result.Read<Person>().ToList();
                var animals = result.Read<Animal>().ToList();
                var people2 = result.Read<Person>().ToList();

                Assert.Equal(4, people.Count);
                Assert.Equal(2, animals.Count);
                Assert.Equal(1, people2.Count);
            }
        }
    }
}