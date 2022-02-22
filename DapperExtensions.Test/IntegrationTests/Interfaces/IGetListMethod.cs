namespace DapperExtensions.Test.IntegrationTests.Interfaces
{
    public interface IGetListMethod
    {
        void UsingNullPredicate_ReturnsAll();
        void UsingObject_ReturnsMatching();
        void UsingPredicate_ReturnsMatching();
    }
}