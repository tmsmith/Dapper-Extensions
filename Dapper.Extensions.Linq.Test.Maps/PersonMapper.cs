using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Test.Entities;

namespace Dapper.Extensions.Linq.Test.Maps
{
    public class PersonMapper : AutoClassMapper<Person>
    {
        public PersonMapper()
        {
            Table("Person");
        }
    }
}