using System;
using System.Linq;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Attributes;
using Dapper.Extensions.Linq.Core.Enums;

namespace Dapper.Extensions.Linq.Mapper
{
    /// <summary>
    /// Automatically maps an entity to a table using a combination of reflection and naming conventions for keys.
    /// Allows for the addition of attributes.
    /// 
    /// <see cref="IgnoreAttribute"/>, <see cref="MapToAttribute"/>, <see cref="PrefixForColumnsAttribute"/>, <see cref="TableNameAttribute"/>.
    /// </summary>
    public class AutoClassMapper<T> : ClassMapper<T> where T : class
    {
        public AutoClassMapper()
        {
            AutoMap();
        }

        public override void Table(string tableName)
        {
            if (Attribute.IsDefined(EntityType, typeof(TableNameAttribute)))
                tableName = ((TableNameAttribute)EntityType.GetCustomAttribute(typeof(TableNameAttribute))).Name;
            base.Table(tableName);
        }

        public override void Schema(string schemaName)
        {
            if (Attribute.IsDefined(EntityType, typeof(SchemaAttribute)))
                schemaName = ((SchemaAttribute)EntityType.GetCustomAttribute(typeof(SchemaAttribute))).Name;
            base.Table(schemaName);
        }

        private void AutoMap()
        {
            Type type = typeof(T);

            string columnPrefix = string.Empty;

            if (Attribute.IsDefined(type, typeof(PrefixForColumnsAttribute)))
                columnPrefix = ((PrefixForColumnsAttribute)type.GetCustomAttribute(typeof(PrefixForColumnsAttribute))).Prefix;

            foreach (PropertyInfo propertyInfo in EntityType.GetProperties())
            {
                PropertyMap propertyMap;
                if (Attribute.IsDefined(propertyInfo, typeof(IgnoreAttribute)))
                    propertyMap = Map(propertyInfo, false).Ignore();
                else if (Attribute.IsDefined(propertyInfo, typeof(MapToAttribute)))
                    propertyMap = Map(propertyInfo, false).Column(((MapToAttribute)propertyInfo.GetCustomAttribute(typeof(MapToAttribute))).DatabaseColumn);
                else if (string.IsNullOrEmpty(columnPrefix) == false)
                    propertyMap = Map(propertyInfo, false).Column(string.Format("{0}{1}", columnPrefix, propertyInfo.Name));
                else propertyMap = Map(propertyInfo, false);

                if (Properties.Any(e => e.KeyType != KeyType.NotAKey)) continue;

                if (string.Equals(propertyMap.PropertyInfo.Name, "id", StringComparison.OrdinalIgnoreCase))
                    propertyMap.Key(PropertyTypeKeyTypeMapping.ContainsKey(propertyMap.PropertyInfo.PropertyType) ?
                        PropertyTypeKeyTypeMapping[propertyMap.PropertyInfo.PropertyType] :
                        KeyType.Assigned);
            }
        }

    }
}