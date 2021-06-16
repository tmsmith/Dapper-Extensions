using DapperExtensions.Mapper;

namespace DapperExtensions.Test.Data.Common
{
    public class Multikey
    {
        public long Key1 { get; set; }
        public string Key2 { get; set; }
        public string Value { get; set; }
    }

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