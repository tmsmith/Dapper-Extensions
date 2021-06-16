using System;
using System.Reflection;

namespace DapperExtensions.Mapper
{
    public class PropertyKey
    {
        public string Name { get; }
        public PropertyInfo PropertyInfo { get; }
        public Type EntityType { get; }

        public PropertyKey(PropertyInfo propertyInfo, Type entityType, string name)
        {
            Name = name;
            PropertyInfo = propertyInfo;
            EntityType = entityType;
        }
    }
}
