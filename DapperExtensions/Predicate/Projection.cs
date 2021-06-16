namespace DapperExtensions.Predicate
{
    public interface IProjection
    {
        string PropertyName { get; }
    }

    public class Projection : IProjection
    {
        public Projection(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
    }
}
