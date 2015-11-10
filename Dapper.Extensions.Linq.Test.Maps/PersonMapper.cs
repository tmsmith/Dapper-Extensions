using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Test.Entities;

namespace Dapper.Extensions.Linq.Test.Maps
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