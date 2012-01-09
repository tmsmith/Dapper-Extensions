using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DapperExtensions
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

                PropertyMap map = Map(propertyInfo);

                if (!keyFound && map.PropertyInfo.Name.EndsWith("id", true, CultureInfo.InvariantCulture))
                {
                    if (map.PropertyInfo.PropertyType == typeof(int) || map.PropertyInfo.PropertyType == typeof(int?))
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

    /// <summary>
    /// Automatically maps an entity to a table using a combination of reflection and naming conventions for keys.
    /// </summary>
    public class AutoClassMapper<T> : ClassMapper<T> where T : class
    {
        public AutoClassMapper()
        {
            Type type = typeof(T);
            Table(type.Name);
            AutoMap();
        }
    }

    /// <summary>
    /// Automatically maps an entity to a table using a combination of reflection and naming conventions for keys. 
    /// Identical to AutoClassMapper, but attempts to pluralize table names automatically.
    /// Example: Person entity maps to People table
    /// </summary>
    public class PluralizedAutoClassMapper<T> : AutoClassMapper<T> where T : class
    {
        public override void Table(string tableName)
        {
            base.Table(Formatting.Pluralize(tableName));
        }
        
        // Adapted from: http://mattgrande.wordpress.com/2009/10/28/pluralization-helper-for-c/
        private static class Formatting
        {
            private static readonly IList<string> Unpluralizables = new List<string> { "equipment", "information", "rice", "money", "species", "series", "fish", "sheep", "deer" };
            private static readonly IDictionary<string, string> Pluralizations = new Dictionary<string, string>
            {
                // Start with the rarest cases, and move to the most common
                { "person", "people" },
                { "ox", "oxen" },
                { "child", "children" },
                { "foot", "feet" },
                { "tooth", "teeth" },
                { "goose", "geese" },
                // And now the more standard rules.
                { "(.*)fe?", "$1ves" },         // ie, wolf, wife
                { "(.*)man$", "$1men" },
                { "(.+[aeiou]y)$", "$1s" },
                { "(.+[^aeiou])y$", "$1ies" },
                { "(.+z)$", "$1zes" },
                { "([m|l])ouse$", "$1ice" },
                { "(.+)(e|i)x$", @"$1ices"},    // ie, Matrix, Index
                { "(octop|vir)us$", "$1i"},
                { "(.+(s|x|sh|ch))$", @"$1es"},
                { "(.+)", @"$1s" }
            };

            public static string Pluralize(string singular)
            {
                if (Unpluralizables.Contains(singular))
                    return singular;

                var plural = string.Empty;

                foreach (var pluralization in Pluralizations)
                {
                    if (Regex.IsMatch(singular, pluralization.Key))
                    {
                        plural = Regex.Replace(singular, pluralization.Key, pluralization.Value);
                        break;
                    }
                }

                return plural;
            }
        }
    }
}