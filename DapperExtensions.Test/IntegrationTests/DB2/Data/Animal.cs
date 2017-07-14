using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DapperExtensions.Mapper;

namespace DapperExtensions.Test.IntegrationTests.DB2.Data
{
    class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class AnimalMapper : ClassMapper<Animal>
    {
        public AnimalMapper()
        {
            Table("ANIMAL");
            Map(a => a.Id).Column("ID").Key(KeyType.Identity);
            Map(a => a.Name).Column("NAME");
        }
    }
}