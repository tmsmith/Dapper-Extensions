using System;
using System.Data;
using System.Reflection;

namespace DapperExtensions.Mapper
{
    /// <summary>
    /// Maps an entity property to its corresponding column in the database.
    /// </summary>
    public interface IPropertyMap
    {
        string Name { get; }
        string ColumnName { get; }
        bool Ignored { get; }
        bool IsReadOnly { get; }

        DbType? DbType { get; }
        ParameterDirection? DbDirection { get; }
        int? DbSize { get; }
        byte? DbPrecision { get; }
        byte? DbScale { get; }

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

        /// <summary>
        /// Gets the name of the property by using the specified propertyInfo.
        /// </summary>
        public string Name
        {
            get { return PropertyInfo.Name; }
        }

        /// <summary>
        /// Gets the column name for the current property.
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Gets the key type for the current property.
        /// </summary>
        public KeyType KeyType { get; private set; }

        /// <summary>
        /// Gets the ignore status of the current property. If ignored, the current property will not be included in queries.
        /// </summary>
        public bool Ignored { get; private set; }

        /// <summary>
        /// Gets the read-only status of the current property. If read-only, the current property will not be included in INSERT and UPDATE queries.
        /// </summary>
        public bool IsReadOnly { get; private set; }


        /// <summary>
        /// Gets the underlying Database Type for the current property.
        /// </summary>
        public DbType? DbType { get; private set; }

        /// <summary>
        /// Gets the parameter direction for the current property
        /// </summary>
        public ParameterDirection? DbDirection { get; private set; }

        /// <summary>
        /// Gets the field length of the current property.
        /// </summary>
        public int? DbSize { get; private set; }

        /// <summary>
        /// Gets the field precision of the current property
        /// </summary>
        public byte? DbPrecision { get; private set; }

        /// <summary>
        /// Gets the field scale of the current property
        /// </summary>
        public byte? DbScale { get; private set; }

        /// <summary>
        /// Gets the property info for the current property.
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Fluently sets the column name for the property.
        /// </summary>
        /// <param name="columnName">The column name as it exists in the database.</param>
        public PropertyMap Column(string columnName)
        {
            ColumnName = columnName;
            return this;
        }

        /// <summary>
        /// Fluently sets the key type of the property.
        /// </summary>
        /// <param name="columnName">The column name as it exists in the database.</param>
        public PropertyMap Key(KeyType keyType)
        {
            if (Ignored)
            {
                throw new ArgumentException(string.Format("'{0}' is ignored and cannot be made a key field. ", Name));
            }

            if (IsReadOnly)
            {
                throw new ArgumentException(string.Format("'{0}' is readonly and cannot be made a key field. ", Name));
            }

            KeyType = keyType;
            return this;
        }

        /// <summary>
        /// Fluently sets the ignore status of the property.
        /// </summary>
        public PropertyMap Ignore()
        {
            if (KeyType != KeyType.NotAKey)
            {
                throw new ArgumentException(string.Format("'{0}' is a key field and cannot be ignored.", Name));
            }

            Ignored = true;
            return this;
        }

        /// <summary>
        /// Fluently sets the read-only status of the property.
        /// </summary>
        public PropertyMap ReadOnly()
        {
            if (KeyType != KeyType.NotAKey)
            {
                throw new ArgumentException(string.Format("'{0}' is a key field and cannot be marked readonly.", Name));
            }

            IsReadOnly = true;
            return this;
        }

        /// <summary>
        /// Fluently sets the field length of the property
        /// </summary>
        /// <param name="size">The length of the field as it exists in the database</param>
        public PropertyMap Size(int size)
        {
            if (size < 0)
            {
                throw new ArgumentException(string.Format("'{0}' cannot have a negative field length.", Name));
            }

            DbSize = size;
            return this;
        }

        /// <summary>
        /// Fluently sets the DbType of the property.
        /// </summary>
        public PropertyMap Type(DbType dbType)
        {
            DbType = dbType;
            return this;
        }

        /// <summary>
        /// Fluently sets the ParameterDirection of the property
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>The expected parameter direction of the current property</returns>
        public PropertyMap Direction(ParameterDirection direction)
        {
            DbDirection = direction;
            return this;
        }

        /// <summary>
        /// Fluently sets the Precision of the property
        /// </summary>
        /// <param name="precision"></param>
        /// <returns></returns>
        public PropertyMap Precision(byte precision)
        {
            DbPrecision = precision;
            return this;
        }

        /// <summary>
        /// Fluently sets the Scale of the current property
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public PropertyMap Scale(byte scale)
        {
            DbScale = scale;
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
        /// The property is an identity generated by the database trigger.
        /// </summary>
        TriggerIdentity,

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