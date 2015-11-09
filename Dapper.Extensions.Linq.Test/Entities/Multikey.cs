using Dapper.Extensions.Linq.Core;

namespace Dapper.Extensions.Linq.Test.Entities
{
    public class Multikey : IEntity
    {
        public int Key1 { get; set; } 
        public string Key2 { get; set; }
        public string Value { get; set; }
        //public DateTime Date { get; set; }
    }
}