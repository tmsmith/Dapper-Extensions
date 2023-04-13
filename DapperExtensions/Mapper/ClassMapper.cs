using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace DapperExtensions.Mapper
{
    public interface IClassMapper
    {
        string SchemaName { get; }
        string TableName { get; }
        string SimpleAlias { get; }
        IList<IMemberMap> Properties { get; }
        IList<IReferenceMap> References { get; }
        Type EntityType { get; }
        Guid Identity { get; }
        Guid ParentIdentity { get; }
        void SetIdentity(Guid identity);
        void SetParentIdentity(Guid identity);
    }

    public interface IClassMapper<T> : IClassMapper
    {
    }

    /// <summary>
    /// Maps an entity to a table through a collection of property maps.
    /// </summary>
    public class ClassMapper<T> : IClassMapper<T>
    {
        /// <summary>
        /// Gets or sets the schema to use when referring to the corresponding table name in the database.
        /// </summary>
        public string SchemaName { get; protected set; }

        /// <summary>
        /// Gets or sets the table to use in the database.
        /// </summary>
        public string TableName { get; protected set; }

        public string SimpleAlias { get; protected set; }

        /// <summary>
        /// A collection of properties that will map to columns in the database table.
        /// </summary>
        public IList<IMemberMap> Properties { get; }

        /// <summary>
        /// A collection of references that will map to columns in the database table.
        /// </summary>
        public IList<IReferenceMap> References { get; }

        public Type EntityType { get; private set; }

        public Guid Identity { get; private set; }
        public Guid ParentIdentity { get; private set; }

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
            References = new List<IReferenceMap>();
            TableName = typeof(T).Name;
            Identity = Guid.NewGuid();
            EntityType = typeof(T);
        }

        protected Dictionary<Type, KeyType> PropertyTypeKeyTypeMapping { get; }

        public virtual void Schema(string schemaName)
        {
            SchemaName = schemaName;
        }

        public virtual void Table(string tableName)
        {
            TableName = tableName;
        }

        protected virtual void AutoMap()
        {
            AutoMap(null);
        }

        protected virtual void AutoMap(Func<Type, PropertyInfo, bool> canMap)
        {
            var type = typeof(T);
            var hasDefinedKey = Properties.Any(p => p.KeyType != KeyType.NotAKey);
            MemberMap keyMap = null;
            foreach (var propertyInfo in type.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public))
            {
                if (Properties.Any(p => p.Name.Equals(propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                if (canMap != null && !canMap(type, propertyInfo))
                {
                    continue;
                }

                var map = Map(propertyInfo);
                if (!hasDefinedKey)
                {
                    if (string.Equals(map.Name, "id", StringComparison.InvariantCultureIgnoreCase))
                    {
                        keyMap = map;
                    }

                    if (keyMap == null && map.Name.EndsWith("id", true, CultureInfo.InvariantCulture))
                    {
                        keyMap = map;
                    }
                }
            }

            keyMap?.Key(PropertyTypeKeyTypeMapping.ContainsKey(keyMap.MemberType)
                    ? PropertyTypeKeyTypeMapping[keyMap.MemberType]
                    : KeyType.Assigned);
        }

        protected virtual IReferenceMap<T> ReferenceMap(Expression<Func<T, object>> expression)
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            var result = new ReferenceMap<T>(propertyInfo, Identity);

            GuardForDuplicateReferenceMap(result);
            References.Add(result);
            return result;
        }

        /// <summary>
        /// Fluently, maps an entity property to a column
        /// </summary>
        protected virtual MemberMap Map(Expression<Func<T, object>> expression)
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression);
            MemberMap result = null;

            if (propertyInfo is IEnumerable<MemberInfo>)
            {
                foreach (var prop in (propertyInfo as IEnumerable<MemberInfo>))
                {
                    result = Map(prop as PropertyInfo, result);
                }
            }
            else
            {
                result = Map(propertyInfo as PropertyInfo);
            }
            return result;
        }

        /// <summary>
        /// Fluently, maps an entity property to a column
        /// </summary>
        protected virtual MemberMap Map(PropertyInfo propertyInfo, MemberMap parent = null)
        {
            var result = new MemberMap(propertyInfo, this, parent: parent);
            if (GuardForDuplicatePropertyMap(result))
            {
                result = (MemberMap)Properties.FirstOrDefault(p => p.Name.Equals(result.Name) && p.ParentProperty == result.ParentProperty);
            }
            else
            {
                Properties.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Removes a propertymap entry
        /// </summary>
        /// <param name="expression"></param>
        protected virtual void UnMap(Expression<Func<T, object>> expression)
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            var mapping = Properties.SingleOrDefault(w => w.Name == propertyInfo.Name);

            if (mapping == null)
            {
                throw new ApplicationException("Unable to UnMap because mapping does not exist.");
            }

            this.Properties.Remove(mapping);
        }

        private bool GuardForDuplicatePropertyMap(MemberMap result)
        {
            return Properties.Any(p => p.Name.Equals(result.Name) && p.ParentProperty == result.ParentProperty);
        }

        private void GuardForDuplicateReferenceMap(ReferenceMap<T> result)
        {
            if (References.Any(p => p.Name.Equals(result.Name)))
            {
                throw new ArgumentException($"Duplicate mapping for reference property {result.Name} detected.");
            }
        }

        public virtual void SetEntityType(Type type)
        {
            EntityType = type;
        }

        public virtual void SetIdentity(Guid identity)
        {
            Identity = identity;
        }

        public virtual void SetParentIdentity(Guid identity)
        {
            ParentIdentity = identity;
        }
    }
}
