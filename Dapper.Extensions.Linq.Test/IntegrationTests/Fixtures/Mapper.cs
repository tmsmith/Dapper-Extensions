using System;
using System.Collections.Generic;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Core.Repositories;
using Dapper.Extensions.Linq.Extensions;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Test.Entities;
using NUnit.Framework;

namespace Dapper.Extensions.Linq.Test.IntegrationTests.Fixtures
{
    public abstract partial class FixturesBase
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

        private class EntityWithMapperMapper : AutoClassMapper<EntityWithMapper>
        {
            public EntityWithMapperMapper()
            {
                Map(p => p.Key).Column("EntityKey").Key(KeyType.Assigned);
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
        }
    }
}