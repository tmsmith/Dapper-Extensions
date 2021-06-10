begin
	EXECUTE IMMEDIATE 'create user xe identified by xe';
	EXECUTE IMMEDIATE 'grant connect to xe';
	EXECUTE IMMEDIATE 'grant dba to xe';
end;