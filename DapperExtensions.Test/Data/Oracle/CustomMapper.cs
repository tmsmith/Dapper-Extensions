using DapperExtensions.Mapper;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Test.Data.Oracle
{
    public class CustomMapper : PluralizedAutoClassMapper<Foo>
    {
        public CustomMapper() : base()
        {
            TableName = "FooTable";
            Map(f => f.Id).Column("Id").Key(KeyType.TriggerIdentity);
            //Map(f => f.DateOfBirth).Column("BirthDate");
            //Map(f => f.FirstName).Column("First");
            //Map(f => f.LastName).Column("Last");
            //Map(f => f.FullName).Ignore();

            Map(f => f.BarList).Ignore();

            ReferenceMap(f => f.BarList).Reference<Bar>((bar, foo) => bar.FooId == foo.Id);
        }
    }

    public class CustomBarMapper : PluralizedAutoClassMapper<Bar>
    {
        public CustomBarMapper() : base()
        {
            TableName = "Bar";
            Map(f => f.Id).Column("BarId").Key(KeyType.TriggerIdentity);
            //Map(f => f.FooId).Column("FooId");
            Map(f => f.Name).Column("BarName");
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

        public IList<Bar> BarList { get; set; }
    }

    public class Bar
    {
        public long Id { get; set; }
        public long FooId { get; set; }
        public string Name { get; set; }
    }
}