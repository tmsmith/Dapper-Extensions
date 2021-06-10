CREATE DATABASE dapper CONTAINMENT = NONE COLLATE Latin1_General_CI_AI_WS;

USE dapper;

CREATE LOGIN dapperExtensions WITH PASSWORD = 'p@ssw0rd', DEFAULT_DATABASE = dapper;

CREATE USER dapperExtensions FOR LOGIN  dapperExtensions WITH DEFAULT_SCHEMA = dapper;

ALTER SERVER ROLE [sysadmin] ADD MEMBER [dapperExtensions];

GRANT ALL PRIVILEGES TO dapperExtensions;
