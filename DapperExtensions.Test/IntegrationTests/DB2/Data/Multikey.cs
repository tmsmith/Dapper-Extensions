using System;
using System.Collections.Generic;
using System.Linq;
using DapperExtensions.Mapper;

namespace DapperExtensions.Test.IntegrationTests.DB2.Data
{
    public class Multikey
    {
        public int Key1 { get; set; }
        public string Key2 { get; set; }
        public string Value { get; set; }
    }

    public class MultikeyMapper : ClassMapper<Multikey>
    {
        public MultikeyMapper()
        {
            Table("MULTIKEY");
            Map(m => m.Key1).Column("KEY1").Key(KeyType.Identity);
            Map(m => m.Key2).Column("KEY2").Key(KeyType.Assigned);
            Map(m => m.Value).Column("VALUE");
        }
    }
}