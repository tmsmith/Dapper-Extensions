using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Mapper;

namespace Dapper.Extensions.Linq.Mapper
{

    /// <summary>
    /// Maps an entity property to its corresponding column in the database.
    /// </summary>
    public class PropertyMap : IPropertyMap
    {
        static readonly Dictionary<Type, DbType> typeMap = new Dictionary<Type, DbType>(){
            {typeof(byte), DbType.Byte},
            {typeof(sbyte), DbType.SByte},
            {typeof(short), DbType.Int16},
            {typeof(ushort), DbType.UInt16},
            {typeof(int), DbType.Int32},
            {typeof(uint), DbType.UInt32},
            {typeof(long), DbType.Int64},
            {typeof(ulong), DbType.UInt64},
            {typeof(float), DbType.Single},
            {typeof(double), DbType.Double},
            {typeof(decimal), DbType.Decimal},
            {typeof(bool), DbType.Boolean},
            {typeof(string), DbType.String},
            {typeof(char), DbType.StringFixedLength},
            {typeof(Guid), DbType.Guid},
            {typeof(DateTime), DbType.DateTime},
            {typeof(DateTimeOffset), DbType.DateTimeOffset},
            {typeof(TimeSpan), DbType.Time},
            {typeof(byte[]), DbType.Binary},
            {typeof(byte?), DbType.Byte},
            {typeof(sbyte?), DbType.SByte},
            {typeof(short?), DbType.Int16},
            {typeof(ushort?), DbType.UInt16},
            {typeof(int?), DbType.Int32},
            {typeof(uint?), DbType.UInt32},
            {typeof(long?), DbType.Int64},
            {typeof(ulong?), DbType.UInt64},
            {typeof(float?), DbType.Single},
            {typeof(double?), DbType.Double},
            {typeof(decimal?), DbType.Decimal},
            {typeof(bool?), DbType.Boolean},
            {typeof(char?), DbType.StringFixedLength},
            {typeof(Guid?), DbType.Guid},
            {typeof(DateTime?), DbType.DateTime},
            {typeof(DateTimeOffset?), DbType.DateTimeOffset},
            {typeof(TimeSpan?), DbType.Time},
            {typeof(Object), DbType.Object}
        };

        public PropertyMap(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            ColumnName = PropertyInfo.Name;
            Type = LookupDbType(propertyInfo.PropertyType, propertyInfo.Name);
        }

        internal const string LinqBinary = "System.Data.Linq.Binary";
        private static DbType LookupDbType(Type type, string name)
        {
            DbType dbType;
            var nullUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullUnderlyingType != null) type = nullUnderlyingType;
            if (type.IsEnum && !typeMap.ContainsKey(type))
            {
                type = Enum.GetUnderlyingType(type);
            }
            if (typeMap.TryGetValue(type, out dbType))
            {
                return dbType;
            }
            if (type.FullName == LinqBinary)
            {
                return DbType.Binary;
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return (DbType)(-1);
            }

            return DbType.Object;
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
        /// Gets the property info for the current property.
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        public DbType Type { get; private set; }

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

        public void SetType(DbType type)
        {
            Type = type;
        }
    }
}