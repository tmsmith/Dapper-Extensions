﻿IF (OBJECT_ID('ph_Phone') IS NOT NULL)
BEGIN
    DROP TABLE ph_Phone
END

CREATE TABLE ph_Phone (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [p_Value] NVARCHAR(50)
)