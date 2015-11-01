using System;
using System.Linq;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Attributes;

namespace Dapper.Extensions.Linq.Mapper
{
    /// <summary>
    /// Automatically maps an entity to a table using a combination of reflection and naming conventions for keys.
    /// 
    /// <see cref="IgnoreAttribute"/> and <see cref="MapToAttribute"/> can be used.
    /// </summary>
    public class AutoClassMapper<T> : ClassMapper<T> where T : class
    {
        public AutoClassMapper()
        {
            Type type = typeof(T);
            Table(type.Name);
            AutoMap();

            foreach (PropertyInfo propertyInfo in EntityType.GetProperties())
            {
                if (Properties.Any(p => p.Name.Equals(propertyInfo.Name))) continue;

                if (Attribute.IsDefined(propertyInfo, typeof(IgnoreAttribute)))
                    Map(propertyInfo)
                        .Ignore();
                else
                {
                    Map(propertyInfo)
                        .Column(Attribute.IsDefined(propertyInfo, typeof(MapToAttribute))
                        ? ((MapToAttribute)propertyInfo.GetCustomAttribute(typeof(MapToAttribute))).DatabaseColumn
                        : propertyInfo.Name);
                }
            }
        }
    }
}