using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Test.Entities;

namespace Dapper.Extensions.Linq.Test.Maps
{
    public class ExternallyMappedMap
    {
        public class ExternallyMappedMapper : ClassMapper<ExternallyMapped>
        {
            public ExternallyMappedMapper()
            {
                Table("External");
                Map(x => x.Id).Column("ExternalId");
                AutoMap();
            }
        } 
    }
}