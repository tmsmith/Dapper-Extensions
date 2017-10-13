using System;
using System.Collections.Generic;
using DapperExtensions.Mapper;

namespace DapperExtensions.Test.IntegrationTests.DB2.Data
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateCreated { get; set; }
        public short Active { get; set; }
        public IEnumerable<Phone> Phones { get; private set; }
    }

    public class Phone
    {
        public int Id { get; set; }
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
            Map(p => p.Phones).Ignore();
        }
    }
}