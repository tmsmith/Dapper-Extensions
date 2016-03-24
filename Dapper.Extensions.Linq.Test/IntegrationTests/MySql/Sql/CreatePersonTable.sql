﻿DROP TABLE IF EXISTS Person;

CREATE TABLE Person (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    DateCreated DATETIME,
    Active BIT,
	ProfileId INT NULL
)