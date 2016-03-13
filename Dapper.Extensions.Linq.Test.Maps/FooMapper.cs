using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Mapper;
using Dapper.Extensions.Linq.Test.Entities;

namespace Dapper.Extensions.Linq.Test.Maps
{
    public class FooMapper : ClassMapper<Foo>
    {
        public FooMapper()
        {
            Schema("dbo");
            Table("FooTable");
            Map(f => f.Id).Column("FooId").Key(KeyType.Identity);
            Map(f => f.DateOfBirth).Column("BirthDate");
            Map(f => f.FirstName).Column("First");
            Map(f => f.LastName).Column("Last");
            Map(f => f.FullName).Ignore();
            Map(f => f.BarList).Ignore();
        }
    }
}