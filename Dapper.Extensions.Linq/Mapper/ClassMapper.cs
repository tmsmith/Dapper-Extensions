using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Logging;
using Dapper.Extensions.Linq.Core.Mapper;

namespace Dapper.Extensions.Linq.Mapper
{
    /// <summary>
    /// Maps an entity to a table through a collection of property maps.
    /// </summary>
    public class ClassMapper<T> : IClassMapper<T> where T : class
    {
        private readonly ILog _log = LogManager.GetLogger<ClassMapper<T>>();

        /// <summary>
        /// Gets or sets the schema to use when referring to the corresponding table name in the database.
        /// </summary>
        public string SchemaName { get; private set; }

        /// <summary>
        /// Gets or sets the table to use in the database.
        /// </summary>
        public string TableName { get; protected set; }

        /// <summary>
        /// A collection of properties that will map to columns in the database table.
        /// </summary>
        public IList<IPropertyMap> Properties { get; }

        public Type EntityType => typeof(T);

        protected ClassMapper()
        {
            _log.InfoFormat("Mapping class: {0}", EntityType.Name);

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
            Schema(null);
            Table(typeof(T).Name);
        }

        protected Dictionary<Type, KeyType> PropertyTypeKeyTypeMapping { get; private set; }

        protected virtual void Schema(string schemaName)
        {
            SchemaName = schemaName;
        }

        public virtual void Table(string tableName)
        {
            TableName = tableName;
        }

        /// <summary>
        /// Fluently, maps an entity property to a column
        /// </summary>
        protected PropertyMap Map(Expression<Func<T, object>> expression, bool overwrite = true)
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return Map(propertyInfo, overwrite);
        }

        /// <summary>
        /// Fluently, maps an entity property to a column
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="overwrite">If an existing map is present, overwrite it</param>
        /// <returns></returns>
        protected PropertyMap Map(PropertyInfo propertyInfo, bool overwrite = true)
        {
            PropertyMap result = new PropertyMap(propertyInfo);

            if (_log.IsDebugEnabled)
                _log.DebugFormat("Mapped property: {0} > {1}, type: {2}", result.Name, result.ColumnName, result.KeyType.GetType().Name);

            IPropertyMap property = Properties.SingleOrDefault(p => p.Name.Equals(result.Name));
            if (overwrite && property != null)
            {
                Properties.Remove(property);
            }
            else if (property != null)
                throw new ArgumentException(string.Format("Duplicate mapping for property {0} detected.", result.Name));

            Properties.Add(result);
            return result;
        }
    }
}