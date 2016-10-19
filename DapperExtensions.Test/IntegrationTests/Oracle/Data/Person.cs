using System;
using System.Collections.Generic;
using DapperExtensions.Mapper;

namespace DapperExtensions.Test.IntegrationTests.Oracle.Data
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Active { get; set; }
        public DateTime DateCreated { get; set; }
        public IEnumerable<Phone> Phones { get; private set; }
    }

    public class Phone
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class PersonOraMap : ClassMapper<Person>
    {
        public PersonOraMap()
        {
            Table("Person");
            Map(x => x.Id).Key(KeyType.TriggerIdentity);
            Map(x => x.Phones).Ignore();
            AutoMap();
        }
    }
}