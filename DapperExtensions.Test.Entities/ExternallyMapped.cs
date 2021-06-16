using System;
using System.Diagnostics.CodeAnalysis;

namespace DapperExtensions.Test.Entities
{
    [ExcludeFromCodeCoverage]
    public class ExternallyMapped
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Active { get; set; }
    }
}