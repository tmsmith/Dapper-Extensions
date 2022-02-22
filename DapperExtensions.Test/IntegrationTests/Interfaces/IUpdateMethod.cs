namespace DapperExtensions.Test.IntegrationTests.Interfaces
{
    public interface IUpdateMethod
    {
        void UsingCompositeKey_UpdatesEntity();
        void UsingKey_UpdatesEntity();
        void UsingGuidKey_UpdatesEntity();
    }
}