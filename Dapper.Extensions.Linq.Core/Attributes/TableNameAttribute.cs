using System;

namespace Dapper.Extensions.Linq.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TableNameAttribute : Attribute
    {
        public string Name { get; private set; }
        public TableNameAttribute(string name)
        {
            Name = name;
        }
    }
}