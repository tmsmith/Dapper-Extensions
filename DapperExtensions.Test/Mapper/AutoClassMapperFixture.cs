using DapperExtensions.Mapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DapperExtensions.Test.Mapper
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class AutoClassMapperFixture
    {
        [TestFixture]
        public class IListTests
        {
            [ExcludeFromCodeCoverage]
            public class Foo
            {
                public IList<Bar> Bars { get; set; }
            }

            [ExcludeFromCodeCoverage]
            public class Bar
            {
                public string Name { get; set; }
            }

            public class FooClassMapper : AutoClassMapper<Foo>
            {
                public FooClassMapper()
                    : base()
                {
                    Map(f => f.Bars).Ignore();
                }
            }

            private static bool MappingIsIgnored(FooClassMapper mapper)
            {
                return mapper.Properties.Any(w => w.Name == "Bars" && w.Ignored);
            }

            [Test]
            public void IListIsIgnored()
            {
                var target = new FooClassMapper();

                Assert.IsTrue(MappingIsIgnored(target));
            }
        }

        [TestFixture]
        public class AutoClassMapperTableName
        {
            [Test]
            public void Constructor_ReturnsProperName()
            {
                AutoClassMapper<Foo> m = GetMapper<Foo>();
                Assert.AreEqual("Foo", m.TableName);
            }

            [Test]
            public void SettingTableName_ReturnsProperName()
            {
                AutoClassMapper<Foo> m = GetMapper<Foo>();
                m.Table("Barz");
                Assert.AreEqual("Barz", m.TableName);
            }

            [Test]
            public void Sets_IdPropertyToKeyWhenFirstProperty()
            {
                AutoClassMapper<IdIsFirst> m = GetMapper<IdIsFirst>();
                var map = m.Properties.Single(p => p.KeyType == KeyType.Guid);
                Assert.IsTrue(map.ColumnName == "Id");
            }

            [Test]
            public void Sets_IdPropertyToKeyWhenFoundInClass()
            {
                AutoClassMapper<IdIsSecond> m = GetMapper<IdIsSecond>();
                var map = m.Properties.Single(p => p.KeyType == KeyType.Guid);
                Assert.IsTrue(map.ColumnName == "Id");
            }

            [Test]
            public void Sets_IdFirstPropertyEndingInIdWhenNoIdPropertyFound()
            {
                AutoClassMapper<IdDoesNotExist> m = GetMapper<IdDoesNotExist>();
                var map = m.Properties.Single(p => p.KeyType == KeyType.Guid);
                Assert.IsTrue(map.ColumnName == "SomeId");
            }

            private static AutoClassMapper<T> GetMapper<T>() where T : class
            {
                return new AutoClassMapper<T>();
            }
        }

        [TestFixture]
        public class CustomAutoMapperTableName
        {
            [Test]
            public void ReturnsProperPluralization()
            {
                CustomAutoMapper<Foo> m = GetMapper<Foo>();
                Assert.AreEqual("Foo", m.TableName);
            }

            [Test]
            public void ReturnsProperResultsForExceptions()
            {
                CustomAutoMapper<Foo2> m = GetMapper<Foo2>();
                Assert.AreEqual("TheFoo", m.TableName);
            }

            private static CustomAutoMapper<T> GetMapper<T>() where T : class
            {
                return new CustomAutoMapper<T>();
            }

            public class CustomAutoMapper<T> : AutoClassMapper<T> where T : class
            {
                public override void Table(string tableName)
                {
                    if (tableName.Equals("Foo2", StringComparison.CurrentCultureIgnoreCase))
                    {
                        TableName = "TheFoo";
                    }
                    else
                    {
                        base.Table(tableName);
                    }
                }
            }
        }

        [ExcludeFromCodeCoverage]
        private class Foo
        {
            public Guid Id { get; set; }
            public Guid ParentId { get; set; }
        }

        [ExcludeFromCodeCoverage]
        private class Foo2
        {
            public Guid ParentId { get; set; }
            public Guid Id { get; set; }
        }

        [ExcludeFromCodeCoverage]
        private class IdIsFirst
        {
            public Guid Id { get; set; }
            public Guid ParentId { get; set; }
        }

        [ExcludeFromCodeCoverage]
        private class IdIsSecond
        {
            public Guid ParentId { get; set; }
            public Guid Id { get; set; }
        }

        [ExcludeFromCodeCoverage]
        private class IdDoesNotExist
        {
            public Guid SomeId { get; set; }
            public Guid ParentId { get; set; }
        }
    }
}
