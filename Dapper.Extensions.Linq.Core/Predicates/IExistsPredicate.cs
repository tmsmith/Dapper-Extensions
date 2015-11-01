namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IExistsPredicate : IPredicate
    {
        IPredicate Predicate { get; set; }
        bool Not { get; set; }
    }
}