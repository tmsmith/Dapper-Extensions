using Dapper.Extensions.Linq.Core;

namespace Dapper.Extensions.Linq.Test.Entities
{
    public class Phone : IEntity
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
}