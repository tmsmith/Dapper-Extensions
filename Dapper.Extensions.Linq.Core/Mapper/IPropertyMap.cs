using System.Data;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Enums;

namespace Dapper.Extensions.Linq.Core.Mapper
{
    /// <summary>
    /// Maps an entity property to its corresponding column in the database.
    /// </summary>
    public interface IPropertyMap
    {
        string Name { get; }
        string ColumnName { get; }
        bool Ignored { get; }
        bool IsReadOnly { get; }
        KeyType KeyType { get; }
        PropertyInfo PropertyInfo { get; }
        DbType Type { get; }
    }
}