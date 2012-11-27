CREATE TABLE Person (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    DateCreated DATETIME,
    Active BIT
)