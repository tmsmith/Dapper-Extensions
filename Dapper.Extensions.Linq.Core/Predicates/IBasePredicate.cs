namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IBasePredicate : IPredicate
    {
        string PropertyName { get; set; }
    }
}