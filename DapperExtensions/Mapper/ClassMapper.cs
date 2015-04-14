using System.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DapperExtensions.Mapper
{
    public interface IClassMapper
    {
        string SchemaName { get; }
        string TableName { get; }
        IList<IMemberMap> Properties { get; }
        Type EntityType { get; }
    }

    public interface IClassMapper<T> : IClassMapper where T : class
    {
    }

    /// <summary>
    /// Maps an entity to a table through a collection of property maps.
    /// </summary>
    public class ClassMapper<T> : IClassMapper<T> where T : class
    {
        /// <summary>
        /// Gets or sets the schema to use when referring to the corresponding table name in the database.
        /// </summary>
        public string SchemaName { get; protected set; }

        /// <summary>
        /// Gets or sets the table to use in the database.
        /// </summary>
        public string TableName { get; protected set; }

        /// <summary>
        /// A collection of properties that will map to columns in the database table.
        /// </summary>
        public IList<IMemberMap> Properties { get; private set; }

        public Type EntityType
        {
            get { return typeof(T); }
        }

        public ClassMapper()
        {
            PropertyTypeKeyTypeMapping = new Dictionary<Type, KeyType>
                                             {
                                                 { typeof(byte), KeyType.Identity }, { typeof(byte?), KeyType.Identity },
                                                 { typeof(sbyte), KeyType.Identity }, { typeof(sbyte?), KeyType.Identity },
                                                 { typeof(short), KeyType.Identity }, { typeof(short?), KeyType.Identity },
                                                 { typeof(ushort), KeyType.Identity }, { typeof(ushort?), KeyType.Identity },
                                                 { typeof(int), KeyType.Identity }, { typeof(int?), KeyType.Identity },
                                                 { typeof(uint), KeyType.Identity}, { typeof(uint?), KeyType.Identity },
                                                 { typeof(long), KeyType.Identity }, { typeof(long?), KeyType.Identity },
                                                 { typeof(ulong), KeyType.Identity }, { typeof(ulong?), KeyType.Identity },
                                                 { typeof(BigInteger), KeyType.Identity }, { typeof(BigInteger?), KeyType.Identity },
                                                 { typeof(Guid), KeyType.Guid }, { typeof(Guid?), KeyType.Guid },
                                             };

            Properties = new List<IMemberMap>();
            SetTableName(typeof(T).Name);
        }

        protected Dictionary<Type, KeyType> PropertyTypeKeyTypeMapping { get; private set; }

        public virtual void SetSchemaName(string schemaName)
        {
            SchemaName = schemaName;
        }

        public virtual void SetTableName(string tableName)
        {
            TableName = tableName;
        }

        protected virtual void AutoMap()
        {
            AutoMap(null);
        }

        protected virtual void AutoMap(Func<Type, PropertyInfo, bool> canMap)
        {
            Type type = typeof(T);
            bool hasDefinedKey = Properties.Any(p => p.KeyType != KeyType.NotAKey);
            MemberMap keyMap = null;
            foreach (var propertyInfo in type.GetProperties())
            {
                if (Properties.Any(p => p.Name.Equals(propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                if ((canMap != null && !canMap(type, propertyInfo)))
                {
                    continue;
                }

                MemberMap map = Map(propertyInfo);
                if (!hasDefinedKey)
                {
                    if (string.Equals(map.MemberInfo.Name, "id", StringComparison.InvariantCultureIgnoreCase))
                    {
                        keyMap = map;
                    }

                    if (keyMap == null && map.MemberInfo.Name.EndsWith("id", true, CultureInfo.InvariantCulture))
                    {
                        keyMap = map;
                    }
                }
            }

            if (keyMap != null)
            {
                keyMap.Key(PropertyTypeKeyTypeMapping.ContainsKey(keyMap.MemberType)
                    ? PropertyTypeKeyTypeMapping[keyMap.MemberType]
                    : KeyType.Assigned);
            }
        }

        /// <summary>
        /// Fluently, maps an entity property to a column
        /// </summary>
        protected MemberMap Map(Expression<Func<T, object>> expression)
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return Map(propertyInfo);
        }

        /// <summary>
        /// Fluently, maps an entity property to a column
        /// </summary>
        protected MemberMap Map(PropertyInfo propertyInfo)
        {
            MemberMap result = new MemberMap(propertyInfo);
            this.GuardForDuplicatePropertyMap(result);
            Properties.Add(result);
            return result;
        }

        private void GuardForDuplicatePropertyMap(MemberMap result)
        {
            if (Properties.Any(p => p.Name.Equals(result.Name)))
            {
                throw new ArgumentException(string.Format("Duplicate mapping for property {0} detected.",result.Name));
            }
        }
    }
}