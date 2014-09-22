using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DapperExtensions.Mapper;

namespace DapperExtensions.Test.Data
{
    public class Dyno
    {
        public dynamic Id { get; set; }
        public string Name { get; set; }
    }

    public class DynoMapper : ClassMapper<Dyno>
    {
        public DynoMapper()
        {
            Table("Dyno");
            Map(x => x.Id).Key(KeyType.Identity);
            AutoMap();
        }
    }
}
