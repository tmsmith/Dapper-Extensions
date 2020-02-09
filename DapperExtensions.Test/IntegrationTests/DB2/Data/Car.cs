using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DapperExtensions.Mapper;

namespace DapperExtensions.Test.IntegrationTests.DB2.Data
{
    class Car
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    class CarMapper : ClassMapper<Car>
    {
        public CarMapper()
        {
            Table("CAR");
            Map(c => c.Id).Column("ID").Key(KeyType.Assigned);
            Map(c => c.Name).Column("NAME");
        }
    }
}