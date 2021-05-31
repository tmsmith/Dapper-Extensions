using DapperExtensions.Mapper;

namespace DapperExtensions.Test.Data.DB2
{
    public class Animal
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class AnimalMapper : ClassMapper<Animal>
    {
        public AnimalMapper()
        {
            Table("ANIMAL");
            Map(a => a.Id).Column("ID").Key(KeyType.Identity);
            Map(a => a.Name).Column("NAME");
        }
    }
}