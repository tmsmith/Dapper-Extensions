using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DapperExtensions.Mapper
{
    /// <summary>
    /// Maps an entity property / field to its corresponding column in the database.
    /// </summary>
    public interface IMemberMap
    {
        string Name { get; }
        string ColumnName { get; }
        bool Ignored { get; }
        bool IsReadOnly { get; }
        KeyType KeyType { get; }
		MemberInfo MemberInfo { get; }
		object GetValue(object obj);
		void SetValue(object obj, object value);
		Type MemberType { get; }
    }

    /// <summary>
    /// Maps an entity property to its corresponding column in the database.
    /// </summary>
    public class MemberMap : IMemberMap
    {
		public MemberMap(PropertyInfo memberInfo)
		{
			MemberInfo = memberInfo;
			ColumnName = MemberInfo.Name;
		}

		public MemberMap(FieldInfo memberInfo)
		{
			MemberInfo = memberInfo;
			ColumnName = MemberInfo.Name;
		}

        /// <summary>
        /// Gets the name of the property by using the specified propertyInfo.
        /// </summary>
        public string Name
        {
            get { return MemberInfo.Name; }
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
        /// Gets the property info for the current property.
        /// </summary>
		public MemberInfo MemberInfo { get; private set; }

        /// <summary>
        /// Fluently sets the column name for the property.
        /// </summary>
        /// <param name="columnName">The column name as it exists in the database.</param>
        public MemberMap Column(string columnName)
        {
            ColumnName = columnName;
            return this;
        }

        /// <summary>
        /// Fluently sets the key type of the property.
        /// </summary>
        /// <param name="columnName">The column name as it exists in the database.</param>
        public MemberMap Key(KeyType keyType)
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
        public MemberMap Ignore()
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
        public MemberMap ReadOnly()
        {
            if (KeyType != KeyType.NotAKey)
            {
                throw new ArgumentException(string.Format("'{0}' is a key field and cannot be marked readonly.", Name));
            }

            IsReadOnly = true;
            return this;
        }

		public object GetValue(object obj)
		{
			if (MemberInfo is FieldInfo)
			{
				return ((FieldInfo)MemberInfo).GetValue(obj);
			}
			else
			{
				return ((PropertyInfo) MemberInfo).GetValue(obj, null);
			}
		}

		public void SetValue(object obj, object value)
		{
			if (MemberInfo is FieldInfo)
			{
				((FieldInfo) MemberInfo).SetValue(obj, value);
			}
			else
			{
				((PropertyInfo) MemberInfo).SetValue(obj, value, null);
			}
		}

		public Type MemberType
		{
			get
			{
				if (MemberInfo is FieldInfo)
				{
					return ((FieldInfo) MemberInfo).FieldType;
				}
				else
				{
					return ((PropertyInfo) MemberInfo).PropertyType;
				}
			}
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