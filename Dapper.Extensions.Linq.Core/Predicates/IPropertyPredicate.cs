namespace Dapper.Extensions.Linq.Core.Predicates
{
    public interface IPropertyPredicate : IComparePredicate
    {
        string PropertyName2 { get; set; }
    }
}