CREATE TABLE FooTable (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,  -- In SQLITE3, this is the alias for ROWID
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    DateOfBirth DATETIME
    /*PRIMARY KEY(Key1, Key2)*/                 -- SQLite3 does not support an autoincrement in a composite key
)