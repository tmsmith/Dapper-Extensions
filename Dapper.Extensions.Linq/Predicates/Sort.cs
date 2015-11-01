using Dapper.Extensions.Linq.Core.Predicates;

namespace Dapper.Extensions.Linq.Predicates
{
    public class Sort : ISort
    {
        public string PropertyName { get; set; }
        public bool Ascending { get; set; }
    }
}