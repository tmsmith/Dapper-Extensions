using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace DapperExtensions.Mapper
{
    /// <summary>
    /// Automatically maps an entity to a table using a combination of reflection and naming conventions for keys.
    /// </summary>
    public class AutoClassMapper<T> : ClassMapper<T> where T : class
    {
        public AutoClassMapper(string tableName = null)
        {
            Type type = typeof(T);
            Table(tableName == null ? type.Name : tableName);
            AutoMap();
        }
    }
}