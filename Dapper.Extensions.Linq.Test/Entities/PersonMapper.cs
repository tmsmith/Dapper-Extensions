using Dapper.Extensions.Linq.Mapper;

namespace Dapper.Extensions.Linq.Test.Entities
{
    public class PersonMapper : ClassMapper<Person>
    {
        public PersonMapper()
        {
            Table("Person");
            Map(m => m.Phones).Ignore();
            AutoMap();
        }
    }
}