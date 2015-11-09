using Dapper.Extensions.Linq.Core;

namespace Dapper.Extensions.Linq.Test.Entities
{
    class Car : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
