connect system/oracle as sysdba;
select * from dba_users;
create user xe identified by xe;
grant connect to xe;
grant dba to xe;
select * from dba_users;
