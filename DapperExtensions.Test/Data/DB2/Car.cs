using DapperExtensions.Mapper;

namespace DapperExtensions.Test.Data.DB2
{
    public class Car
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class CarMapper : ClassMapper<Car>
    {
        public CarMapper()
        {
            Table("CAR");
            Map(c => c.Id).Column("ID").Key(KeyType.Assigned);
            Map(c => c.Name).Column("NAME");
        }
    }
}