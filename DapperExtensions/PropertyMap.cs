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
        bool Ignored { get; }
        bool IsReadOnly { get; }
        KeyType KeyType { get; }
        PropertyInfo PropertyInfo { get; }
    }

    /// <summary>
    /// Maps an entity property to its corresponding column in the database.
    /// </summary>
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
        public bool Ignored { get; private set; }
        public bool IsReadOnly { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }

        public PropertyMap Column(string columnName)
        {
            ColumnName = columnName;
            return this;
        }

        public PropertyMap Key(KeyType keyType)
        {
            if (Ignored)
            {
                throw new ArgumentException(string.Format("'{0}' is ignored and cannot be made a key field. ", Name));
            }

            if (Ignored)
            {
                throw new ArgumentException(string.Format("'{0}' is readonly and cannot be made a key field. ", Name));
            }

            KeyType = keyType;
            return this;
        }

        public PropertyMap Ignore()
        {
            if (KeyType != KeyType.NotAKey)
            {
                throw new ArgumentException(string.Format("'{0}' is a key field and cannot be ignored.", Name));
            }

            Ignored = true;
            return this;
        }

        public PropertyMap ReadOnly()
        {
            if (KeyType != KeyType.NotAKey)
            {
                throw new ArgumentException(string.Format("'{0}' is a key field and cannot be marked readonly.", Name));
            }

            IsReadOnly = true;
            return this;
        }
    }

    /// <summary>
    /// Used by ClassMapper to determine which entity property represents the key.
    /// </summary>
    public enum KeyType
    {
        /// <summary>
        /// The property is not a key and is not automatically managed.
        /// </summary>
        NotAKey,

        /// <summary>
        /// The property is an integery-based identity generated from the database.
        /// </summary>
        Identity,

        /// <summary>
        /// The property is a Guid identity which is automatically managed.
        /// </summary>
        Guid,

        /// <summary>
        /// The property is a key that is not automatically managed.
        /// </summary>
        Assigned
    }
}