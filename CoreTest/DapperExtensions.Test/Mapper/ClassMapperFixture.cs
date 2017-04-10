using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using DapperExtensions.Mapper;
using DapperExtensions.Test.Helpers;
using Moq;
using Xunit;

namespace DapperExtensions.Test.Mapper
{
    public class ClassMapperFixture
    {
        public abstract class ClassMapperFixtureBase
        {
            public void Setup()
            {
            }

            protected ClassMapper<T> GetMapper<T>() where T : class
            {
                return new ClassMapper<T>();
            }
        }

        public class AutoMapIdTests : ClassMapperFixtureBase
        {
            [Fact]
            public void AutoMapSetsFirstIdToIdentityWhenByte()
            {
                var mapper = GetMapper<Test1<byte>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<byte?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Fact]
            public void AutoMapSetsFirstIdToIdentityWhenSByte()
            {
                var mapper = GetMapper<Test1<sbyte>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<sbyte?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Fact]
            public void AutoMapSetsFirstIdToIdentityWhenInt16()
            {
                var mapper = GetMapper<Test1<short>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<short?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Fact]
            public void AutoMapSetsFirstIdToIdentityWhenUnsignedInt16()
            {
                var mapper = GetMapper<Test1<ushort>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<ushort?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Fact]
            public void AutoMapSetsFirstIdToIdentityWhenInt32()
            {
                var mapper = GetMapper<Test1<int>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<int?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Fact]
            public void AutoMapSetsFirstIdToIdentityWhenUnsignedInt32()
            {
                var mapper = GetMapper<Test1<uint>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<uint?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Fact]
            public void AutoMapSetsFirstIdToIdentityWhenInt64()
            {
                var mapper = GetMapper<Test1<long>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<long?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Fact]
            public void AutoMapSetsFirstIdToIdentityWhenUnsignedInt64()
            {
                var mapper = GetMapper<Test1<ulong>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<ulong?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Fact]
            public void AutoMapSetsFirstIdToIdentityWhenBigInteger()
            {
                var mapper = GetMapper<Test1<BigInteger>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<BigInteger?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Fact]
            public void AutoMapSetsFirstIdToGuidWhenGuid()
            {
                var mapper = GetMapper<Test1<Guid>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Guid, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<Guid?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Guid, mapper2.Properties[0].KeyType);
            }

            [Fact]
            public void AutoMapSetsFirstIdToAssignedWhenNotKeyType()
            {
                var mapper = GetMapper<Test1<string>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Assigned, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<bool>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Assigned, mapper2.Properties[0].KeyType);

                var mapper3 = GetMapper<Test1<bool?>>();
                mapper3.TestProtected().RunMethod("AutoMap");
                Assert.Equal(KeyType.Assigned, mapper3.Properties[0].KeyType);
            }

            private class Test1<T>
            {
                public T SomeId { get; set; }
            }
        }

        public class AutoMapMethod : ClassMapperFixtureBase
        {
            [Fact]
            public void MapsAllProperties()
            {
                var mapper = GetMapper<FooWithIntId>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(3, mapper.Properties.Count);
                Assert.Equal(mapper.Properties[0].ColumnName, "FooId");
                Assert.Equal(mapper.Properties[0].Name, "FooId");
                Assert.Equal(mapper.Properties[1].ColumnName, "Value");
                Assert.Equal(mapper.Properties[1].Name, "Value");
                Assert.Equal(mapper.Properties[2].ColumnName, "BarId");
                Assert.Equal(mapper.Properties[2].Name, "BarId");
            }

            [Fact]
            public void MakesFirstIntId_AIdentityKey()
            {
                var mapper = GetMapper<FooWithIntId>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(mapper.Properties[0].KeyType, KeyType.Identity);
                Assert.Equal(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Fact]
            public void MakesFirstGuidId_AGuidKey()
            {
                var mapper = GetMapper<FooWithGuidId>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(mapper.Properties[0].KeyType, KeyType.Guid);
                Assert.Equal(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Fact]
            public void MakesFirstStringId_AAssignedKey()
            {
                var mapper = GetMapper<FooWithStringId>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(mapper.Properties[0].KeyType, KeyType.Assigned);
                Assert.Equal(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Fact]
            public void DoesNotMapAlreadyMappedProperties()
            {
                Mock<IPropertyMap> property = new Mock<IPropertyMap>();
                property.SetupGet(p => p.Name).Returns("FooId");
                property.SetupGet(p => p.KeyType).Returns(KeyType.Assigned);

                var mapper = GetMapper<FooWithIntId>();
                mapper.Properties.Add(property.Object);
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(3, mapper.Properties.Count);
                Assert.Equal(mapper.Properties[0], property.Object);
                Assert.Equal(mapper.Properties[1].KeyType, KeyType.NotAKey);
                Assert.Equal(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Fact]
            public void EnumerableDoesNotThrowException()
            {
                var mapper = GetMapper<Foo>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(2, mapper.Properties.Count);
            }

            [Fact]
            public void IgnoringAnEnumerableDoesNotCauseError()
            {
                var mapper = new TestMapper<Foo>();
                mapper.Map(m => m.List).Ignore();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.Equal(2, mapper.Properties.Count);
            }

            [Fact]
            public void DoesNotMapPropertyWhenCanMapIsFalse()
            {
                var mapper = new TestMapper<Foo>();
                Func<Type, PropertyInfo, bool> canMap = (t, p) => ReflectionHelper.IsSimpleType(p.PropertyType);
                mapper.TestProtected().RunMethod("AutoMap", canMap);
                Assert.Equal(1, mapper.Properties.Count);                
            }
        }

        public class FooWithIntId
        {
            public int FooId { get; set; }
            public string Value { get; set; }
            public int BarId { get; set; }
        }

        public class FooWithGuidId
        {
            public Guid FooId { get; set; }
            public string Value { get; set; }
            public Guid BarId { get; set; }
        }

        public class FooWithStringId
        {
            public string FooId { get; set; }
            public string Value { get; set; }
            public string BarId { get; set; }
        }

        public class Foo
        {
            public int FooId { get; set; }
            public IEnumerable<string> List { get; set; }
        }

        public class TestMapper<T> : ClassMapper<T> where T : class
        {
            public PropertyMap Map(Expression<Func<T, object>> expression)
            {
                return base.Map(expression);
            }
        }
    }
}