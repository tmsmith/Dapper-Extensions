using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Mapper;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.Mapper
{
    [TestFixture]
    public class ClassMapperFixture
    {
        public abstract class ClassMapperFixtureBase
        {
            [SetUp]
            public void Setup()
            {
            }

            protected AutoClassMapper<T> GetAutoMapper<T>() where T : class
            {
                return new AutoClassMapper<T>();
            }
        }

        [TestFixture]
        public class AutoMapMethod : ClassMapperFixtureBase
        {
            [Test]
            public void MapsAllProperties()
            {
                var mapper = GetAutoMapper<FooWithIntId>();
                Assert.AreEqual(3, mapper.Properties.Count);
                Assert.AreEqual(mapper.Properties[0].ColumnName, "Id");
                Assert.AreEqual(mapper.Properties[0].Name, "Id");
                Assert.AreEqual(mapper.Properties[1].ColumnName, "Value");
                Assert.AreEqual(mapper.Properties[1].Name, "Value");
                Assert.AreEqual(mapper.Properties[2].ColumnName, "BarId");
                Assert.AreEqual(mapper.Properties[2].Name, "BarId");
            }

            [Test]
            public void MakesFirstIntId_AIdentityKey()
            {
                var mapper = GetAutoMapper<FooWithIntId>();
                Assert.AreEqual(mapper.Properties[0].KeyType, KeyType.Identity);
                Assert.AreEqual(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Test]
            public void MakesFirstGuidId_AGuidKey()
            {
                var mapper = GetAutoMapper<FooWithGuidId>();
                Assert.AreEqual(mapper.Properties[0].KeyType, KeyType.Guid);
                Assert.AreEqual(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Test]
            public void MakesFirstStringId_AAssignedKey()
            {
                var mapper = GetAutoMapper<FooWithStringId>();
                Assert.AreEqual(mapper.Properties[0].KeyType, KeyType.Assigned);
                Assert.AreEqual(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Test]
            public void EnumerableDoesNotThrowException()
            {
                var mapper = GetAutoMapper<Foo>();
                Assert.AreEqual(2, mapper.Properties.Count);
            }

            [Test]
            public void IgnoringAnEnumerableDoesNotCauseError()
            {
                var mapper = new TestMapper<Foo>();
                mapper.Map(m => m.List).Ignore();
                Assert.AreEqual(2, mapper.Properties.Count);
            }
        }

        public class FooWithIntId
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public int BarId { get; set; }
        }

        public class FooWithGuidId
        {
            public Guid Id { get; set; }
            public string Value { get; set; }
            public Guid BarId { get; set; }
        }

        public class FooWithStringId
        {
            public string Id { get; set; }
            public string Value { get; set; }
            public string BarId { get; set; }
        }

        public class Foo
        {
            public int FooId { get; set; }
            public IEnumerable<string> List { get; set; }
        }

        public class TestMapper<T> : AutoClassMapper<T> where T : class
        {
            public new PropertyMap Map(Expression<Func<T, object>> expression)
            {
                return base.Map(expression);
            }
        }
    }
}