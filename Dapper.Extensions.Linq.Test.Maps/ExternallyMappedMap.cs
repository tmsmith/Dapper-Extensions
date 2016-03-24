using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Test.Entities;

namespace Dapper.Extensions.Linq.Test.Maps
{
    public class ExternallyMappedMap
    {
        public class ExternallyMappedMapper : AutoClassMapper<ExternallyMapped>
        {
            public ExternallyMappedMapper()
            {
                Table("External");
                Map(x => x.Id).Column("ExternalId");
            }
        } 
    }
}