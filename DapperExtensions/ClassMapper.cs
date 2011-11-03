using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

    public class ClassMapper<T> : IClassMapper<T> where T : class
    {
        public string SchemaName { get; private set; }
        public string TableName { get; private set; }
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
                    if (map.PropertyInfo.PropertyType == typeof(int))
                    {
                        map.Key(KeyType.Identity);
                    }
                    else if (map.PropertyInfo.PropertyType == typeof(Guid))
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

        protected PropertyMap Map(Expression<Func<T, object>> expression)
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            return Map(propertyInfo);
        }

        protected PropertyMap Map(PropertyInfo propertyInfo)
        {
            PropertyMap result = new PropertyMap(propertyInfo);
            Properties.Add(result);
            return result;
        }
    }

    public class AutoClassMapper<T> : ClassMapper<T> where T : class
    {
        public AutoClassMapper()
        {
            Type type = typeof(T);
            Table(type.Name);
            AutoMap();
        }
    }

    public class PlurizedAutoClassMapper<T> : AutoClassMapper<T> where T : class
    {
        public override void Table(string tableName)
        {
            base.Table(tableName + "s");
        }
    }
}