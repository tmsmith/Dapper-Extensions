using DapperExtensions.Mapper;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Test.Data.DB2
{
    public class Person
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateCreated { get; set; }
        public short Active { get; set; }
        public IEnumerable<Phone> Phones { get; }
    }

    public class Phone
    {
        public long Id { get; set; }
        public string Value { get; set; }
    }

    public class PersonMapper : ClassMapper<Person>
    {
        public PersonMapper()
        {
            Table("PERSON");
            Map(p => p.Id).Column("ID").Key(KeyType.Identity);
            Map(p => p.FirstName).Column("FIRSTNAME");
            Map(p => p.LastName).Column("LASTNAME");
            Map(p => p.DateCreated).Column("DATECREATED");
            Map(p => p.Active).Column("ACTIVE");
            //Map(p => p.Phones).Ignore();
        }
    }
}