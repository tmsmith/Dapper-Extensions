using DapperExtensions.Mapper;

namespace DapperExtensions.Test.Data
{
    public class MultikeyMapper : ClassMapper<Multikey>
    {
        public MultikeyMapper()
        {
            Map(p => p.Key1).Key(KeyType.Identity);
            Map(p => p.Key2).Key(KeyType.Assigned);
            AutoMap();
        }
    }
}