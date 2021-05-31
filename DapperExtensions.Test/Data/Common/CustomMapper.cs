using DapperExtensions.Mapper;
using System;

namespace DapperExtensions.Test.Data.Common
{
    public class CustomMapper : ClassMapper<Foo>
    {
        public CustomMapper()
        {
            TableName = "FooTable";
            Map(f => f.Id).Column("FooId").Key(KeyType.Identity);
            Map(f => f.DateOfBirth).Column("BirthDate");
            Map(f => f.FirstName).Column("First");
            Map(f => f.LastName).Column("Last");
            Map(f => f.FullName).Ignore();
            //Map(f => f.BarList).Ignore();
        }
    }

    public class Foo
    {
        public long Id { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }

        //public List<Bar> BarList { get; set; }
    }

    //public class Bar
    //{
    //    public long BarId { get; set; }
    //    public string Name { get; set; }
    //}
}