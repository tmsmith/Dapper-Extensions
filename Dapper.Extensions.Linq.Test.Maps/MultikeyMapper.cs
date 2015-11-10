using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Test.Entities;

namespace Dapper.Extensions.Linq.Test.Maps
{
    public class MultikeyMapper : ClassMapper<Multikey>
    {
        public MultikeyMapper()
        {
            Map(p => p.Key1).Key(KeyType.Identity);
            Map(p => p.Key2).Key(KeyType.Assigned);
            //Map(p => p.Date).Ignore();
            AutoMap();
        }
    }
}