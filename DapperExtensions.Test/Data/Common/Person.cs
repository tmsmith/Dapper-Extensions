using DapperExtensions.Mapper;
using System;
using System.Collections.Generic;

namespace DapperExtensions.Test.Data.Common
{
    public class Person
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Active { get; set; }
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
            TableName = nameof(Person);
            Map(m => m.Phones).Ignore();
            AutoMap();
        }
    }
}