using DapperExtensions.Mapper;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Test.Data.Oracle
{
    public class Person
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Active { get; set; }
        public DateTime DateCreated { get; set; }
        public IEnumerable<Phone> Phones { get; }
    }

    public class Phone
    {
        public long Id { get; set; }
        public string Value { get; set; }
    }

    public class PersonOraMap : ClassMapper<Person>
    {
        public PersonOraMap()
        {
            Table("Person");
            Map(x => x.Id).Key(KeyType.TriggerIdentity);
            //Map(x => x.Phones).Ignore();
            AutoMap();
        }
    }
}