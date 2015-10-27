using System;

namespace Dapper.Extensions.Linq.Test.Entities
{
    public class ExternallyMapped
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Active { get; set; }
    }
}