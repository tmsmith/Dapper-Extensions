using System;
using System.Collections.Generic;
using Dapper.Extensions.Linq.Core;
using Dapper.Extensions.Linq.Core.Attributes;

namespace Dapper.Extensions.Linq.Test.Entities
{
    public class Person : IEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Active { get; set; }

        [Ignore]
        public IEnumerable<Phone> Phones { get; private set; }
    }
}