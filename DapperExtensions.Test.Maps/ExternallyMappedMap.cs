using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DapperExtensions.Mapper;
using DapperExtensions.Test.Entities;

namespace DapperExtensions.Test.Maps
{
    public class ExternallyMappedMap
    {
        public class ExternallyMappedMapper : ClassMapper<ExternallyMapped>
        {
            public ExternallyMappedMapper()
            {
                SetTableName("External");
                Map(x => x.Id).Column("ExternalId");
                AutoMap();
            }
        } 
    }
}