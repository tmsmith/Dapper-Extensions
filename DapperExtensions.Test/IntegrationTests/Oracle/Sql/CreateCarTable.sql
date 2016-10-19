BEGIN
  FOR cur IN (SELECT table_name FROM USER_TABLES WHERE table_name = 'CAR') LOOP
	EXECUTE IMMEDIATE 'DROP TABLE Car';
  END LOOP;

  EXECUTE IMMEDIATE 'CREATE TABLE Car (
                       Id VARCHAR2(15) PRIMARY KEY,
                       Name VARCHAR2(50))';
END;