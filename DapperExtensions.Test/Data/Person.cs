using System;
using System.Collections.Generic;
using DapperExtensions.Mapper;

namespace DapperExtensions.Test.Data
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Active { get; set; }
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
            SetTableName("Person");
            Map(m => m.Phones).Ignore();
            AutoMap();
        }
    }
}