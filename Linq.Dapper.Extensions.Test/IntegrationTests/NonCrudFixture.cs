using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DapperExtensions.Test.Maps;
using Linq.Dapper.Extensions.Mapper;
using Linq.Dapper.Extensions.Test.Entities;
using NUnit.Framework;

namespace Linq.Dapper.Extensions.Test.IntegrationTests
{
    [TestFixture]
    public class NonCrudFixture
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
                    Guid id = Linq.Dapper.Extensions.DapperExtensions.GetNextGuid();
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
                var mapper = Linq.Dapper.Extensions.DapperExtensions.GetMap<EntityWithoutMapper>();
                Assert.AreEqual(typeof(AutoClassMapper<EntityWithoutMapper>), mapper.GetType());
            }

            [Test]
            public void ClassMapperDescendant_Returns_DefinedClass()
            {
                var mapper = Linq.Dapper.Extensions.DapperExtensions.GetMap<EntityWithMapper>();
                Assert.AreEqual(typeof(EntityWithMapperMapper), mapper.GetType());
            }

            [Test]
            public void ClassMapperInterface_Returns_DefinedMapper()
            {
                var mapper = Linq.Dapper.Extensions.DapperExtensions.GetMap<EntityWithInterfaceMapper>();
                Assert.AreEqual(typeof(EntityWithInterfaceMapperMapper), mapper.GetType());
            }

            [Test]
            public void MappingClass_ReturnsFromDifferentAssembly()
            {
                Linq.Dapper.Extensions.DapperExtensions.SetMappingAssemblies(new[] { typeof(ExternallyMappedMap).Assembly });
                var mapper = Linq.Dapper.Extensions.DapperExtensions.GetMap<ExternallyMapped>();
                Assert.AreEqual(typeof(ExternallyMappedMap.ExternallyMappedMapper), mapper.GetType());

                Linq.Dapper.Extensions.DapperExtensions.SetMappingAssemblies(null);
                mapper = Linq.Dapper.Extensions.DapperExtensions.GetMap<ExternallyMapped>();
                Assert.AreEqual(typeof(AutoClassMapper<ExternallyMapped>), mapper.GetType());
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
                public Type EntityType { get; private set; }

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