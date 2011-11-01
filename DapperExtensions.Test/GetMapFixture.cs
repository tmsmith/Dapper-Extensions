using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

namespace DapperExtensions.Test
{
    public class GetMapFixture : BaseFixture
    {
        [Test]
        public void Returns_DefaultMapper_When_No_Mapping_Class()
        {
            var mapper = DapperExtensions.GetMap<EntityWithoutMapper>();
            Assert.AreEqual(typeof(AutoClassMapper<EntityWithoutMapper>), mapper.GetType());
        }

        [Test]
        public void Returns_DefinedMapper_When_Mapping_Class()
        {
            var mapper = DapperExtensions.GetMap<EntityWithMapper>();
            Assert.AreEqual(typeof(EntityWithMapperMapper), mapper.GetType());
        }

        [Test]
        public void Returns_DefinedMapper_When_No_Mapping_Interface()
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