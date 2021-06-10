using DapperExtensions.Extensions;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DapperExtensions.Mapper
{
    /// <summary>
    /// Maps an entity property to its corresponding column in the database.
    /// </summary>
    public interface IMemberMap
    {
        string Name { get; }
        string ColumnName { get; }
        string SequenceName { get; }
        bool Ignored { get; }
        bool IsReadOnly { get; }

        DbType? DbType { get; }
        ParameterDirection? DbDirection { get; }
        int? DbSize { get; }
        byte? DbPrecision { get; }
        byte? DbScale { get; }

        KeyType KeyType { get; }
        MemberInfo MemberInfo { get; }
        IClassMapper ClassMapper { get; }
        IMemberMap ParentProperty { get; }
        object GetValue(object obj);
        void SetValue(object obj, object value);
        Type MemberType { get; }
        bool UseEnumDescription { get; }
    }

    /// <summary>
    /// Maps an entity property to its corresponding column in the database.
    /// </summary>
    public class MemberMap : IMemberMap
    {
        #region Constructors
        public MemberMap(PropertyInfo memberInfo) : this(memberInfo, null)
        {
        }

        public MemberMap(FieldInfo memberInfo) : this(memberInfo, null)
        {
        }

        public MemberMap(MemberInfo memberInfo) : this(memberInfo, null)
        {
        }

        public MemberMap(MemberInfo memberInfo, IClassMapper classMapper, bool isReference = false, IMemberMap parent = null)
        {
            MemberInfo = memberInfo;
            ColumnName = isReference ? classMapper.Properties
                                                  .Where(x => x.MemberInfo == MemberInfo)
                                                  .Select(c => c.ColumnName)
                                                  .FirstOrDefault() : MemberInfo.Name;
            ClassMapper = classMapper;

            ParentProperty = parent;
        }
        #endregion

        public IMemberMap ParentProperty { get; }

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
        /// Gets the sequence name for generate Id for column property
        /// </summary>
        public string SequenceName { get; private set; }

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
        public MemberInfo MemberInfo { get; }

        public IClassMapper ClassMapper { get; }
        /// <summary>
        /// Fluently sets the column name for the property.
        /// </summary>
        /// <param name="columnName">The column name as it exists in the database.</param>
        public MemberMap Column(string columnName)
        {
            ColumnName = columnName;
            return this;
        }

        public MemberMap Sequence(string sequenceName)
        {
            SequenceName = sequenceName;
            if (!string.IsNullOrEmpty(sequenceName))
                Key(KeyType.SequenceIdentity);

            return this;
        }

        /// <summary>
        /// Fluently sets the key type of the property.
        /// </summary>
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

            if (!string.IsNullOrEmpty(SequenceName) && keyType != KeyType.SequenceIdentity)
            {
                throw new ArgumentException(string.Format("'{0}' cannot be made a key field. ", Name));
            }

            KeyType = keyType;
            return this;
        }

        /// <summary>
        /// Fluently sets the ignore status of the property.
        /// </summary>
        public MemberMap Ignore()
        {
            if (KeyType != KeyType.NotAKey && KeyType != KeyType.SlapperIdentifierKey)
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
            if (KeyType != KeyType.NotAKey && KeyType != KeyType.SlapperIdentifierKey)
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
        public MemberMap Size(int size)
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
        public MemberMap Type(DbType dbType)
        {
            DbType = dbType;
            return this;
        }

        /// <summary>
        /// Fluently sets the ParameterDirection of the property
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>The expected parameter direction of the current property</returns>
        public MemberMap Direction(ParameterDirection direction)
        {
            DbDirection = direction;
            return this;
        }

        /// <summary>
        /// Fluently sets the Precision of the property
        /// </summary>
        /// <param name="precision"></param>
        /// <returns></returns>
        public MemberMap Precision(byte precision)
        {
            DbPrecision = precision;
            return this;
        }

        /// <summary>
        /// Fluently sets the Scale of the current property
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public MemberMap Scale(byte scale)
        {
            DbScale = scale;
            return this;
        }

        public object GetValue(object obj)
        {
            if (MemberInfo is FieldInfo info)
            {
                return info.GetValue(obj);
            }
            else if (MemberInfo.DeclaringType.IsEnum)
            {
                return (obj as Enum).Description();
            }
            else
            {
                return ((PropertyInfo)MemberInfo).GetValue(obj, null);
            }
        }

        public void SetValue(object obj, object value)
        {
            if (MemberInfo is FieldInfo info)
            {
                info.SetValue(obj, value);
            }
            else
            {
                ((PropertyInfo)MemberInfo).SetValue(obj, value, null);
            }
        }

        public Type MemberType
        {
            get
            {
                if (MemberInfo is FieldInfo info)
                {
                    return info.FieldType;
                }
                else
                {
                    return ((PropertyInfo)MemberInfo).PropertyType;
                }
            }
        }

        public bool UseEnumDescription { get; private set; }

        public MemberMap EnumDescription(bool value)
        {
            UseEnumDescription = value;
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
        Assigned,
        /// <summary>
        /// The property is a key that represents a property from domain table reference.
        /// </summary>
        ForeignKey,

        /// <summary>
        /// The property is a key generated by the database sequence.
        /// </summary>
        SequenceIdentity,

        /// <summary>
        /// The property is a key that's used for Slapper mapping identifier only.
        /// </summary>
        SlapperIdentifierKey
    }
}