CREATE TABLE Person (
    Id INTEGER PRIMARY KEY, -- In SQLITE3, this is the alias for ROWID
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    DateCreated DATETIME,
    Active BIT
)