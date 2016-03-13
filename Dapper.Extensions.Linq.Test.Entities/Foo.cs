using System;
using System.Collections.Generic;
using Dapper.Extensions.Linq.Core;

namespace Dapper.Extensions.Linq.Test.Entities
{
    public class Foo : IEntity
    {
        public int Id { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => string.Format("{0} {1}", FirstName, LastName);
        public List<Bar> BarList { get; set; }
    }

    public class Bar
    {
        public int BarId { get; set; }
        public string Name { get; set; }
    }
}
