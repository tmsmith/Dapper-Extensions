using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperExtensions.Test.Entities
{
    public class ExternallyMapped
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Active { get; set; }
    }
}