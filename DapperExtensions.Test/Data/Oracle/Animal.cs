using DapperExtensions.Mapper;

namespace DapperExtensions.Test.Data.Oracle
{
    public class Animal
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class AnimalOraMap : ClassMapper<Animal>
    {
        public AnimalOraMap()
        {
            Table("Animal");
            Map(x => x.Id).Key(KeyType.TriggerIdentity);
            AutoMap();
        }
    }
}