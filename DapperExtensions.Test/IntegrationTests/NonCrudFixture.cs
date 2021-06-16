using DapperExtensions.Mapper;
using DapperExtensions.Test.Entities;
using DapperExtensions.Test.Maps;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperExtensions.Test.IntegrationTests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public static class NonCrudFixture
    {
        [TestFixture]
        public class GetNextGuidMethod
        {
            [Test]
            public void GetMultiple_DoesNotDuplicate()
            {
                List<Guid> list = new List<Guid>();
                for (int i = 0; i < 1000; i++)
                {
                    Guid id = DapperExtensions.GetNextGuid();
                    Assert.IsFalse(list.Contains(id));
                    list.Add(id);
                }
            }
        }

        [TestFixture]
        public class GetMapMethod
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

            [Test]
            public void MappingClass_ReturnsFromDifferentAssembly()
            {
                DapperExtensions.SetMappingAssemblies(new[] { typeof(ExternallyMappedMap).Assembly });
                var mapper = DapperExtensions.GetMap<ExternallyMapped>();
                Assert.AreEqual(typeof(ExternallyMappedMap.ExternallyMappedMapper), mapper.GetType());

                DapperExtensions.SetMappingAssemblies(null);
                mapper = DapperExtensions.GetMap<ExternallyMapped>();
                Assert.AreEqual(typeof(AutoClassMapper<ExternallyMapped>), mapper.GetType());
            }

            [ExcludeFromCodeCoverage]
            private class EntityWithoutMapper
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            [ExcludeFromCodeCoverage]
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

            [ExcludeFromCodeCoverage]
            private class EntityWithInterfaceMapper
            {
                public string Key { get; set; }
                public string Value { get; set; }
            }

            [ExcludeFromCodeCoverage]
            private class EntityWithInterfaceMapperMapper : IClassMapper<EntityWithInterfaceMapper>
            {
                public string SchemaName { get; }
                public string TableName { get; }
                public IList<IMemberMap> Properties { get; }
                public Type EntityType { get; }

                public string SimpleAlias { get; }

                public IList<IReferenceMap> References { get; }

                public Guid Identity { get; private set; }

                public Guid ParentIdentity { get; private set; }

                public MemberMap Map(Expression<Func<EntityWithInterfaceMapper, object>> expression)
                {
                    throw new NotImplementedException();
                }

                public MemberMap Map(PropertyInfo propertyInfo)
                {
                    throw new NotImplementedException();
                }

                public void SetIdentity(Guid identity)
                {
                    throw new NotImplementedException();
                }

                public void SetParentIdentity(Guid identity)
                {
                    throw new NotImplementedException();
                }
            }
        }

        [TestFixture]
        public class ConfigurationMethods
        {
            [Test]
            public void SetCaseSensitiveSearch_True_ChangesCaseSensitiveSearchEnabled()
            {
                var config = new DapperExtensionsConfiguration();

                config.SetCaseSensitiveSearch(true);

                Assert.IsTrue(config.CaseSensitiveSearchEnabled);
            }

            [Test]
            public void SetCaseSensitiveSearch_False_ChangesCaseSensitiveSearchEnabled()
            {
                var config = new DapperExtensionsConfiguration();

                config.SetCaseSensitiveSearch(false);

                Assert.IsFalse(config.CaseSensitiveSearchEnabled);
            }
        }
    }
}