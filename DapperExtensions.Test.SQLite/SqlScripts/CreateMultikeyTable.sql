CREATE TABLE Multikey (
    Key1 INTEGER PRIMARY KEY,					-- In SQLITE3, this is the alias for ROWID
    Key2 NVARCHAR(50) NOT NULL,					
    Value NVARCHAR(50)
    /*PRIMARY KEY(Key1, Key2)*/                 -- SQLite3 does not support an autoincrement in a composite key
)