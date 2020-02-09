using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using DapperExtensions.Mapper;
using DapperExtensions.Test.Helpers;
using Moq;
using NUnit.Framework;

namespace DapperExtensions.Test.Mapper
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

            protected ClassMapper<T> GetMapper<T>() where T : class
            {
                return new ClassMapper<T>();
            }
        }

        [TestFixture]
        public class UnMapTests : ClassMapperFixtureBase
        {
            public class Foo
            {
                public string Name { get; set; }
            }

            public class FooClassMapper : ClassMapper<Foo>
            {
                public FooClassMapper()
                {
                }

                //hook to access protected methods
                public new PropertyMap Map(Expression<Func<Foo, object>> expression)
                {
                    return base.Map(expression);
                }

                //hook to access protected methods
                public new void UnMap(Expression<Func<Foo, object>> expression)
                {
                    base.UnMap(expression);
                }
            }

            private bool mappingExists(FooClassMapper mapper)
            {
                return mapper.Properties.Where(w => w.Name == "Name").Count() == 1;
            }

            [Test]
            public void UnMapRemovesAnExistingMapping()
            {
                var target = new FooClassMapper();

                target.Map(p => p.Name);
                Assert.IsTrue(mappingExists(target));

                target.UnMap(p => p.Name);
                Assert.IsFalse(mappingExists(target));
            }

            [Test]
            [ExpectedException(typeof(ApplicationException))]
            public void UnMapThrowExceptionWhenMappingDidntPreviouslyExist()
            {
                var target = new FooClassMapper();

                target.UnMap(p => p.Name);
            }
        }

        [TestFixture]
        public class AutoMapIdTests : ClassMapperFixtureBase
        {
            [Test]
            public void AutoMapSetsFirstIdToIdentityWhenByte()
            {
                var mapper = GetMapper<Test1<byte>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<byte?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Test]
            public void AutoMapSetsFirstIdToIdentityWhenSByte()
            {
                var mapper = GetMapper<Test1<sbyte>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<sbyte?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Test]
            public void AutoMapSetsFirstIdToIdentityWhenInt16()
            {
                var mapper = GetMapper<Test1<short>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<short?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Test]
            public void AutoMapSetsFirstIdToIdentityWhenUnsignedInt16()
            {
                var mapper = GetMapper<Test1<ushort>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<ushort?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Test]
            public void AutoMapSetsFirstIdToIdentityWhenInt32()
            {
                var mapper = GetMapper<Test1<int>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<int?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Test]
            public void AutoMapSetsFirstIdToIdentityWhenUnsignedInt32()
            {
                var mapper = GetMapper<Test1<uint>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<uint?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Test]
            public void AutoMapSetsFirstIdToIdentityWhenInt64()
            {
                var mapper = GetMapper<Test1<long>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<long?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Test]
            public void AutoMapSetsFirstIdToIdentityWhenUnsignedInt64()
            {
                var mapper = GetMapper<Test1<ulong>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<ulong?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Test]
            public void AutoMapSetsFirstIdToIdentityWhenBigInteger()
            {
                var mapper = GetMapper<Test1<BigInteger>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<BigInteger?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Identity, mapper2.Properties[0].KeyType);
            }

            [Test]
            public void AutoMapSetsFirstIdToGuidWhenGuid()
            {
                var mapper = GetMapper<Test1<Guid>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Guid, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<Guid?>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Guid, mapper2.Properties[0].KeyType);
            }

            [Test]
            public void AutoMapSetsFirstIdToAssignedWhenNotKeyType()
            {
                var mapper = GetMapper<Test1<string>>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Assigned, mapper.Properties[0].KeyType);

                var mapper2 = GetMapper<Test1<bool>>();
                mapper2.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Assigned, mapper2.Properties[0].KeyType);

                var mapper3 = GetMapper<Test1<bool?>>();
                mapper3.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(KeyType.Assigned, mapper3.Properties[0].KeyType);
            }

            private class Test1<T>
            {
                public T SomeId { get; set; }
            }
        }

        [TestFixture]
        public class AutoMapMethod : ClassMapperFixtureBase
        {
            [Test]
            public void MapsAllProperties()
            {
                var mapper = GetMapper<FooWithIntId>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(3, mapper.Properties.Count);
                Assert.AreEqual(mapper.Properties[0].ColumnName, "FooId");
                Assert.AreEqual(mapper.Properties[0].Name, "FooId");
                Assert.AreEqual(mapper.Properties[1].ColumnName, "Value");
                Assert.AreEqual(mapper.Properties[1].Name, "Value");
                Assert.AreEqual(mapper.Properties[2].ColumnName, "BarId");
                Assert.AreEqual(mapper.Properties[2].Name, "BarId");
            }

            [Test]
            public void MakesFirstIntId_AIdentityKey()
            {
                var mapper = GetMapper<FooWithIntId>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(mapper.Properties[0].KeyType, KeyType.Identity);
                Assert.AreEqual(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Test]
            public void MakesFirstGuidId_AGuidKey()
            {
                var mapper = GetMapper<FooWithGuidId>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(mapper.Properties[0].KeyType, KeyType.Guid);
                Assert.AreEqual(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Test]
            public void MakesFirstStringId_AAssignedKey()
            {
                var mapper = GetMapper<FooWithStringId>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(mapper.Properties[0].KeyType, KeyType.Assigned);
                Assert.AreEqual(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Test]
            public void DoesNotMapAlreadyMappedProperties()
            {
                Mock<IPropertyMap> property = new Mock<IPropertyMap>();
                property.SetupGet(p => p.Name).Returns("FooId");
                property.SetupGet(p => p.KeyType).Returns(KeyType.Assigned);

                var mapper = GetMapper<FooWithIntId>();
                mapper.Properties.Add(property.Object);
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(3, mapper.Properties.Count);
                Assert.AreEqual(mapper.Properties[0], property.Object);
                Assert.AreEqual(mapper.Properties[1].KeyType, KeyType.NotAKey);
                Assert.AreEqual(mapper.Properties[2].KeyType, KeyType.NotAKey);
            }

            [Test]
            public void EnumerableDoesNotThrowException()
            {
                var mapper = GetMapper<Foo>();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(2, mapper.Properties.Count);
            }

            [Test]
            public void IgnoringAnEnumerableDoesNotCauseError()
            {
                var mapper = new TestMapper<Foo>();
                mapper.Map(m => m.List).Ignore();
                mapper.TestProtected().RunMethod("AutoMap");
                Assert.AreEqual(2, mapper.Properties.Count);
            }

            [Test]
            public void DoesNotMapPropertyWhenCanMapIsFalse()
            {
                var mapper = new TestMapper<Foo>();
                Func<Type, PropertyInfo, bool> canMap = (t, p) => ReflectionHelper.IsSimpleType(p.PropertyType);
                mapper.TestProtected().RunMethod("AutoMap", canMap);
                Assert.AreEqual(1, mapper.Properties.Count);                
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