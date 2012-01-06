using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DapperExtensions.Test.Data;
using DapperExtensions.Test.Helpers;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    [TestFixture]
    public class DapperExtensionsImpFixture
    {
        public class InsertMethod : DatabaseConnection
        {
            [Test]
            public void AddsEntityToDatabase_ReturnsKey()
            {
                Person p = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                int id = Impl.Insert(Connection, p, null, null);
                Assert.AreEqual(1, id);
                Assert.AreEqual(1, p.Id);
            }

            [Test]
            public void AddsEntityToDatabase_ReturnsCompositeKey()
            {
                Multikey m = new Multikey { Key2 = "key", Value = "foo" };
                var key = Impl.Insert(Connection, m, null, null);
                Assert.AreEqual(1, key.Key1);
                Assert.AreEqual("key", key.Key2);
            }

            [Test]
            public void AddsEntityToDatabase_ReturnsGeneratedPrimaryKey()
            {
                Animal a1 = new Animal { Name = "Foo" };
                Impl.Insert(Connection, a1, null, null);

                var a2 = Impl.Get<Animal>(Connection, a1.Id, null, null);
                Assert.AreNotEqual(Guid.Empty, a2.Id);
                Assert.AreEqual(a1.Id, a2.Id);
            }

            [Test]
            public void AddsMultipleEntitiesToDatabase()
            {
                Animal a1 = new Animal { Name = "Foo" };
                Animal a2 = new Animal { Name = "Bar" };
                Animal a3 = new Animal { Name = "Baz" };

                Impl.Insert<Animal>(Connection, new[] { a1, a2, a3 }, null, null);

                var animals = Impl.GetList<Animal>(Connection, null, null, null, null, false).ToList();
                Assert.AreEqual(3, animals.Count);
            }
        }

        public class GetMethod : DatabaseConnection
        {
            [Test]
            public void UsingKey_ReturnsEntity()
            {
                Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                int id = Impl.Insert(Connection, p1, null, null);

                Person p2 = Impl.Get<Person>(Connection, id, null, null);
                Assert.AreEqual(id, p2.Id);
                Assert.AreEqual("Foo", p2.FirstName);
                Assert.AreEqual("Bar", p2.LastName);
            }

            [Test]
            public void UsingCompositeKey_ReturnsEntity()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Impl.Insert(Connection, m1, null, null);

                Multikey m2 = Impl.Get<Multikey>(Connection, new { key.Key1, key.Key2 }, null, null);
                Assert.AreEqual(1, m2.Key1);
                Assert.AreEqual("key", m2.Key2);
                Assert.AreEqual("bar", m2.Value);
            }
        }

        public class DeleteMethod : DatabaseConnection
        {
            [Test]
            public void UsingKey_DeletesFromDatabase()
            {
                Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                int id = Impl.Insert(Connection, p1, null, null);

                Person p2 = Impl.Get<Person>(Connection, id, null, null);
                Impl.Delete(Connection, p2, null, null);
                Assert.IsNull(Impl.Get<Person>(Connection, id, null, null));
            }

            [Test]
            public void UsingCompositeKey_DeletesFromDatabase()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Impl.Insert(Connection, m1, null, null);

                Multikey m2 = Impl.Get<Multikey>(Connection, new { key.Key1, key.Key2 }, null, null);
                Impl.Delete(Connection, m2, null, null);
                Assert.IsNull(Impl.Get<Multikey>(Connection, new { key.Key1, key.Key2 }, null, null));
            }

            [Test]
            public void UsingPredicate_DeletesRows()
            {
                Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p2 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                Person p3 = new Person { Active = true, FirstName = "Foo", LastName = "Barz", DateCreated = DateTime.UtcNow };
                Impl.Insert(Connection, p1, null, null);
                Impl.Insert(Connection, p2, null, null);
                Impl.Insert(Connection, p3, null, null);

                var list = Impl.GetList<Person>(Connection, null, null, null, null, false);
                Assert.AreEqual(3, list.Count());

                IPredicate pred = Predicates.Field<Person>(p => p.LastName, Operator.Eq, "Bar");
                var result = Impl.Delete<Person>(Connection, pred, null, null);
                Assert.IsTrue(result);

                list = Impl.GetList<Person>(Connection, null, null, null, null, false);
                Assert.AreEqual(1, list.Count());
            }

        }

        public class UpdateMethod : DatabaseConnection
        {
            [Test]
            public void UsingKey_UpdatesEntity()
            {
                Person p1 = new Person { Active = true, FirstName = "Foo", LastName = "Bar", DateCreated = DateTime.UtcNow };
                int id = Impl.Insert(Connection, p1, null, null);

                var p2 = Impl.Get<Person>(Connection, id, null, null);
                p2.FirstName = "Baz";
                p2.Active = false;

                Impl.Update(Connection, p2, null, null);

                var p3 = Impl.Get<Person>(Connection, id, null, null);
                Assert.AreEqual("Baz", p3.FirstName);
                Assert.AreEqual("Bar", p3.LastName);
                Assert.AreEqual(false, p3.Active);
            }

            [Test]
            public void UsingCompositeKey_UpdatesEntity()
            {
                Multikey m1 = new Multikey { Key2 = "key", Value = "bar" };
                var key = Impl.Insert(Connection, m1, null, null);

                Multikey m2 = Impl.Get<Multikey>(Connection, new { key.Key1, key.Key2 }, null, null);
                m2.Key2 = "key";
                m2.Value = "barz";
                Impl.Update(Connection, m2, null, null);

                Multikey m3 = Impl.Get<Multikey>(Connection, new { Key1 = 1, Key2 = "key" }, null, null);
                Assert.AreEqual(1, m3.Key1);
                Assert.AreEqual("key", m3.Key2);
                Assert.AreEqual("barz", m3.Value);
            }
        }

        public class GetListMethod : DatabaseConnection
        {
            [Test]
            public void UsingNullPredicate_ReturnsAll()
            {
                Impl.Insert(Connection, new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow }, null, null);
                Impl.Insert(Connection, new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow }, null, null);
                Impl.Insert(Connection, new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow }, null, null);
                Impl.Insert(Connection, new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow }, null, null);

                IEnumerable<Person> list = Impl.GetList<Person>(Connection, null, null, null, null, false);
                Assert.AreEqual(4, list.Count());
            }

            [Test]
            public void UsingPredicate_ReturnsMatching()
            {
                Impl.Insert(Connection, new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow }, null, null);
                Impl.Insert(Connection, new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow }, null, null);
                Impl.Insert(Connection, new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow }, null, null);
                Impl.Insert(Connection, new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow }, null, null);

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
                IEnumerable<Person> list = Impl.GetList<Person>(Connection, predicate, null, null, null, false);
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "a" || p.FirstName == "c"));
            }
        }

        public class GetPageMethod : DatabaseConnection
        {
            [Test]
            public void UsingNullPredicate_ReturnsMatching()
            {
                var id1 = Impl.Insert(Connection, new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow }, null, null);
                var id2 = Impl.Insert(Connection, new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow }, null, null);
                var id3 = Impl.Insert(Connection, new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow }, null, null);
                var id4 = Impl.Insert(Connection, new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow }, null, null);

                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                IEnumerable<Person> list = Impl.GetPage<Person>(Connection, null, sort, 1, 2, null, null, false);
                Assert.AreEqual(2, list.Count());
                Assert.AreEqual(id2, list.First().Id);
                Assert.AreEqual(id1, list.Skip(1).First().Id);
            }

            [Test]
            public void UsingPredicate_ReturnsMatching()
            {
                var id1 = Impl.Insert(Connection, new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow }, null, null);
                var id2 = Impl.Insert(Connection, new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow }, null, null);
                var id3 = Impl.Insert(Connection, new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow }, null, null);
                var id4 = Impl.Insert(Connection, new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow }, null, null);

                var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                IEnumerable<Person> list = Impl.GetPage<Person>(Connection, predicate, sort, 1, 3, null, null, false);
                Assert.AreEqual(2, list.Count());
                Assert.IsTrue(list.All(p => p.FirstName == "Sigma" || p.FirstName == "Theta"));
            }

            [Test]
            public void NotFirstPage_Returns_NextResults()
            {
                var id1 = Impl.Insert(Connection, new Person { Active = true, FirstName = "Sigma", LastName = "Alpha", DateCreated = DateTime.UtcNow }, null, null);
                var id2 = Impl.Insert(Connection, new Person { Active = false, FirstName = "Delta", LastName = "Alpha", DateCreated = DateTime.UtcNow }, null, null);
                var id3 = Impl.Insert(Connection, new Person { Active = true, FirstName = "Theta", LastName = "Gamma", DateCreated = DateTime.UtcNow }, null, null);
                var id4 = Impl.Insert(Connection, new Person { Active = false, FirstName = "Iota", LastName = "Beta", DateCreated = DateTime.UtcNow }, null, null);

                IList<ISort> sort = new List<ISort>
                                    {
                                        Predicates.Sort<Person>(p => p.LastName),
                                        Predicates.Sort<Person>(p => p.FirstName)
                                    };

                IEnumerable<Person> list = Impl.GetPage<Person>(Connection, null, sort, 2, 2, null, null, false);
                Assert.AreEqual(2, list.Count());
                Assert.AreEqual(id4, list.First().Id);
                Assert.AreEqual(id3, list.Skip(1).First().Id);
            }
        }

        public class CountMethod : DatabaseConnection
        {
            [Test]
            public void UsingNullPredicate_Returns_Count()
            {
                Impl.Insert(Connection, new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) }, null, null);
                Impl.Insert(Connection, new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) }, null, null);
                Impl.Insert(Connection, new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) }, null, null);
                Impl.Insert(Connection, new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) }, null, null);

                int count = Impl.Count<Person>(Connection, null, null, null);
                Assert.AreEqual(4, count);
            }

            [Test]
            public void UsingPredicate_Returns_Count()
            {
                Impl.Insert(Connection, new Person { Active = true, FirstName = "a", LastName = "a1", DateCreated = DateTime.UtcNow.AddDays(-10) }, null, null);
                Impl.Insert(Connection, new Person { Active = false, FirstName = "b", LastName = "b1", DateCreated = DateTime.UtcNow.AddDays(-10) }, null, null);
                Impl.Insert(Connection, new Person { Active = true, FirstName = "c", LastName = "c1", DateCreated = DateTime.UtcNow.AddDays(-3) }, null, null);
                Impl.Insert(Connection, new Person { Active = false, FirstName = "d", LastName = "d1", DateCreated = DateTime.UtcNow.AddDays(-1) }, null, null);

                var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
                int count = Impl.Count<Person>(Connection, predicate, null, null);
                Assert.AreEqual(2, count);
            }
        }

        public class GetNextGuidMethod : DatabaseConnection
        {
            [Test]
            public void GetMultiple_DoesNotDuplicate()
            {
                List<Guid> list = new List<Guid>();
                for (int i = 0; i < 1000; i++)
                {
                    Guid id = Impl.GetNextGuid();
                    Assert.IsFalse(list.Contains(id));
                    list.Add(id);
                }
            }
        }

        public class GetMapMethod : DatabaseConnection
        {
            [Test]
            public void NoMappingClass_ReturnsDefaultMapper()
            {
                var mapper = DapperExtensions.GetMap<EntityWithoutMapper>();
                Assert.AreEqual(typeof(AutoClassMapper<EntityWithoutMapper>), mapper.GetType());
            }

            [Test]
            public void ClassMapperDescendant_Returns_DefinedClass()
            {
                var mapper = DapperExtensions.GetMap<EntityWithMapper>();
                Assert.AreEqual(typeof(EntityWithMapperMapper), mapper.GetType());
            }

            [Test]
            public void ClassMapperInterface_Returns_DefinedMapper()
            {
                var mapper = DapperExtensions.GetMap<EntityWithInterfaceMapper>();
                Assert.AreEqual(typeof(EntityWithInterfaceMapperMapper), mapper.GetType());
            }

            private class EntityWithoutMapper
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            private class EntityWithMapper
            {
                public string Key { get; set; }
                public string Value { get; set; }
            }

            private class EntityWithMapperMapper : ClassMapper<EntityWithMapper>
            {
                public EntityWithMapperMapper()
                {
                    Map(p => p.Key).Column("EntityKey").Key(KeyType.Assigned);
                    AutoMap();
                }
            }

            private class EntityWithInterfaceMapper
            {
                public string Key { get; set; }
                public string Value { get; set; }
            }

            private class EntityWithInterfaceMapperMapper : IClassMapper<EntityWithInterfaceMapper>
            {
                public string SchemaName { get; private set; }
                public string TableName { get; private set; }
                public IList<IPropertyMap> Properties { get; private set; }
                public PropertyMap Map(Expression<Func<EntityWithInterfaceMapper, object>> expression)
                {
                    throw new NotImplementedException();
                }

                public PropertyMap Map(PropertyInfo propertyInfo)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}