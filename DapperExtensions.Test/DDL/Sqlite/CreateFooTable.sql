CREATE TABLE FooTable (
    FooId INTEGER PRIMARY KEY AUTOINCREMENT,  -- In SQLITE3, this is the alias for ROWID
    [First] NVARCHAR(50),
    [Last] NVARCHAR(50),
    BirthDate DATETIME
    /*PRIMARY KEY(Key1, Key2)*/                 -- SQLite3 does not support an autoincrement in a composite key
)