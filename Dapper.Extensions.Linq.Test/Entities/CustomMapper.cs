using System;
using System.Collections.Generic;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Mapper;

namespace Dapper.Extensions.Linq.Test.Entities
{
    public class CustomMapper : ClassMapper<Foo>
    {
        public CustomMapper()
        {
            Table("FooTable");
            Map(f => f.Id).Column("FooId").Key(KeyType.Identity);
            Map(f => f.DateOfBirth).Column("BirthDate");
            Map(f => f.FirstName).Column("First");
            Map(f => f.LastName).Column("Last");
            Map(f => f.FullName).Ignore();
            Map(f => f.BarList).Ignore();
        }
    }

    public class Foo
    {
        public int Id { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }

        public List<Bar> BarList { get; set; }
    }

    public class Bar
    {
        public int BarId { get; set; }
        public string Name { get; set; }
    }
}