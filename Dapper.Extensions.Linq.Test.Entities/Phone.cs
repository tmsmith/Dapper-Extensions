using Dapper.Extensions.Linq.Core;
using Dapper.Extensions.Linq.Core.Attributes;

namespace Dapper.Extensions.Linq.Test.Entities
{
    [TableName("ph_Phone")]
    [PrefixForColumns("p_")]
    public class Phone : IEntity
    {
        [MapTo("Id")]
        public int Id { get; set; }

        public string Value { get; set; }
    }
}