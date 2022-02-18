namespace DapperExtensions.Test.IntegrationTests.Interfaces
{
    public interface IDeleteMethod
    {
        void UsingCompositeKey_DeletesFromDatabase();
        void UsingKey_DeletesFromDatabase();
        void UsingObject_DeletesRows();
        void UsingPredicate_DeletesRows();
    }
}