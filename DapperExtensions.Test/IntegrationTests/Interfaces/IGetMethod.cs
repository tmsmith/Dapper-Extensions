namespace DapperExtensions.Test.IntegrationTests.Interfaces
{
    public interface IGetMethod
    {
        void UsingCompositeKey_ReturnsEntity();
        void UsingKey_ReturnsEntity();
        void UsingDirectConnection_ReturnsEntity();
        void UsingKey_ReturnsEntityWithRelations();
    }
}