using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DapperExtensions
{
    public interface IPropertyMap
    {
        string Name { get; }
        string ColumnName { get; }
        KeyType KeyType { get; }
        PropertyInfo PropertyInfo { get; }
    }

    public class PropertyMap : IPropertyMap
    {
        public PropertyMap(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            ColumnName = PropertyInfo.Name;
        }

        public string Name
        {
            get { return PropertyInfo.Name; }
        }

        public string ColumnName { get; private set; }
        public KeyType KeyType { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }

        public PropertyMap Column(string columnName)
        {
            ColumnName = columnName;
            return this;
        }

        public PropertyMap Key(KeyType keyType)
        {
            KeyType = keyType;
            return this;
        }
    }

    public enum KeyType
    {
        NotAKey,
        Identity,
        Guid,
        Assigned
    }
}