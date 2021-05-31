using System;
using System.Reflection;

namespace DapperExtensions.Mapper
{
    public class Table
    {
        public Guid Identity { get; set; }
        public Guid ParentIdentity { get; set; }
        public Guid LastIdentity { get; set; }
        public string Alias { get; set; }
        public string ReferenceName { get; set; }
        public string Name { get; set; }
        public Type EntityType { get; set; }
        public Type ParentEntityType { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public bool IsVirtual { get; set; }
        public IClassMapper ClassMapper { get; set; }
    }
}

