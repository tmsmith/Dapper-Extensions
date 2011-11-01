using System;
using System.Collections.Generic;
using System.Linq;

namespace DapperExtensions
{
    public interface IDapperFormatter
    {
        string GetTableName(IClassMapper map);
        string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias);
        string GetColumnName(IClassMapper map, string propertyName, bool includeAlias);
        Guid GetNextGuid();
    }

    public class DefaultFormatter : IDapperFormatter
    {
        public virtual string GetTableName(IClassMapper map)
        {
            string result = (string.IsNullOrWhiteSpace(map.SchemaName) ? null : "[" + map.SchemaName + "].") + "[" + map.TableName + "]";
            return result;
        }

        public virtual string GetColumnName(IClassMapper map, IPropertyMap property, bool includeAlias)
        {
            string result = GetTableName(map) + ".[" + property.ColumnName + "]";
            if (property.ColumnName == property.Name || !includeAlias)
            {
                return result;
            }

            return result + " AS [" + property.Name + "]";
        }

        public virtual string GetColumnName(IClassMapper map, string propertyName, bool includeAlias)
        {
            IPropertyMap propertyMap = map.Properties.Where(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (propertyMap == null)
            {
                throw new ArgumentException("Could not find '{0} in Mapping.");
            }

            return GetColumnName(map, propertyMap, includeAlias);
        }

        public virtual Guid GetNextGuid()
        {
            byte[] b = Guid.NewGuid().ToByteArray();
            DateTime dateTime = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan(now.Ticks - dateTime.Ticks);
            TimeSpan timeOfDay = now.TimeOfDay;
            byte[] bytes1 = BitConverter.GetBytes(timeSpan.Days);
            byte[] bytes2 = BitConverter.GetBytes((long)(timeOfDay.TotalMilliseconds / 3.333333));
            Array.Reverse(bytes1);
            Array.Reverse(bytes2);
            Array.Copy(bytes1, bytes1.Length - 2, b, b.Length - 6, 2);
            Array.Copy(bytes2, bytes2.Length - 4, b, b.Length - 4, 4);
            return new Guid(b);
        }
    }
}