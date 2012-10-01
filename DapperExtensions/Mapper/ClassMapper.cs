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
        IList<IPropertyMap> Properties { get; }
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

        public ClassMapper()
        {
            Properties = new List<IPropertyMap>();
            Table(typeof(T).Name);
        }

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
            Type type = typeof(T);
            bool keyFound = Properties.Any(p => p.KeyType != KeyType.NotAKey);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (Properties.Any(p => p.Name.Equals(propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }
                if (propertyInfo.GetCustomAttributes(typeof(IgnoreAttribute), true).Length > 0)
                {
                    continue;
                }
                PropertyMap map = Map(propertyInfo);

                if (!keyFound && map.PropertyInfo.Name.EndsWith("id", true, CultureInfo.InvariantCulture))
                {
                    if (map.PropertyInfo.PropertyType == typeof(int) || 
                        map.PropertyInfo.PropertyType == typeof(int?) || 
                        map.PropertyInfo.PropertyType == typeof(long) || // SQLite stores all integers as Int64, IDs must follow suit
                        map.PropertyInfo.PropertyType == typeof(long?))
                    {
                        map.Key(KeyType.Identity);
                    }
                    else if (map.PropertyInfo.PropertyType == typeof(Guid) || map.PropertyInfo.PropertyType == typeof(Guid?))
                    {
                        map.Key(KeyType.Guid);
                    }
                    else
                    {
                        map.Key(KeyType.Assigned);
                    }

                    keyFound = true;
                }
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
            Properties.Add(result);
            return result;
        }
    }
}