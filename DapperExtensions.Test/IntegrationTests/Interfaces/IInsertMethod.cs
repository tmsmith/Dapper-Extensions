namespace DapperExtensions.Test.IntegrationTests.Interfaces
{
    public interface IInsertMethod
    {
        void AddsEntityToDatabase_ReturnsCompositeKey();
        void AddsEntityToDatabase_ReturnsGeneratedPrimaryKey();
        void AddsEntityToDatabase_ReturnsKey();
        void AddsMultipleEntitiesToDatabase();
        void AddsEntityToDatabase_WithPassedInGuid();
        void AddsMultipleEntitiesToDatabase_WithPassedInGuid();
    }
}