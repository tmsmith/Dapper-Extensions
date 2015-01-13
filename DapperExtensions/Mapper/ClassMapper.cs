using System.Numerics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DapperExtensions.Attributes;

namespace DapperExtensions.Mapper
{
    public interface IClassMapper
    {
        string SchemaName { get; }
        string TableName { get; }
        IList<IPropertyMap> Properties { get; }
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
        public IList<IPropertyMap> Properties { get; private set; }

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

            Properties = new List<IPropertyMap>();
            Table(typeof(T).Name);
        }

        protected Dictionary<Type, KeyType> PropertyTypeKeyTypeMapping { get; private set; }

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
            Type type = typeof(T);

            foreach (var propertyInfo in type.GetProperties())
            {
                if (AlreadyHasProperty(propertyInfo.Name) || 
                    (canMap != null && !canMap(type, propertyInfo)))
                {
                    continue;
                }

                PropertyMap map = Map(propertyInfo);

                var propertyAttributes = propertyInfo.GetCustomAttributes(typeof(DapperPropertyAttribute), false);
                CheckForIgnore(map, propertyAttributes);
                CheckForReadOnly(map, propertyAttributes);
                CheckForKey(map, propertyAttributes);
            }
        }

        /// <summary>
        /// Fluently, maps an entity property to a column
        /// </summary>
        protected PropertyMap Map(Expression<Func<T, object>> expression)
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return Map(propertyInfo);
        }

        /// <summary>
        /// Fluently, maps an entity property to a column
        /// </summary>
        protected PropertyMap Map(PropertyInfo propertyInfo)
        {
            PropertyMap result = new PropertyMap(propertyInfo);
            this.GuardForDuplicatePropertyMap(result);
            Properties.Add(result);
            return result;
        }

        private void GuardForDuplicatePropertyMap(PropertyMap result)
        {
            if (Properties.Any(p => p.Name.Equals(result.Name)))
            {
                throw new ArgumentException(string.Format("Duplicate mapping for property {0} detected.",result.Name));
            }
        }

        private void CheckForReadOnly(PropertyMap map, IEnumerable<object> propertyAttributes)
        {
            if (propertyAttributes.OfType<DapperReadOnlyAttribute>().Any())
            {
                map.ReadOnly();
            }
        }

        private void CheckForIgnore(PropertyMap map, IEnumerable<object> propertyAttributes)
        {
            if (propertyAttributes.OfType<DapperIgnoreAttribute>().Any())
            {
                map.Ignore();
            }
        }

        private bool AlreadyHasProperty(string name)
        {
            return Properties.Any(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        private void CheckForKey(PropertyMap map, IEnumerable<object> propertyAttributes)
        {
            var keyAttributes = propertyAttributes.OfType<DapperPropertyKeyTypeAttribute>().ToList();

            if (keyAttributes.Any())
            {
                map.Key(keyAttributes[0].KeyType);
            }
            else if (ThereIsNoKeyPropertyYet() && ColumnCanBeKey(map.PropertyInfo.Name))
            {
                map.Key(PropertyTypeKeyTypeMapping.ContainsKey(map.PropertyInfo.PropertyType)
                    ? PropertyTypeKeyTypeMapping[map.PropertyInfo.PropertyType]
                    : KeyType.Assigned);
            }
        }

        private const string IdColumnName = "id";
        private bool ColumnCanBeKey(string name)
        {
            return
                string.Equals(name, IdColumnName, StringComparison.InvariantCultureIgnoreCase) ||
                name.EndsWith(IdColumnName, true, CultureInfo.InvariantCulture);
        }

        private bool ThereIsNoKeyPropertyYet()
        {
            return Properties.All(x => x.KeyType == KeyType.NotAKey);
        }
    }
}