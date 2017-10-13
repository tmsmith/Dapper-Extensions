BEGIN
  FOR cur IN (SELECT table_name FROM USER_TABLES WHERE table_name = 'PERSON') LOOP
	EXECUTE IMMEDIATE 'DROP TABLE Person';
  END LOOP;

  FOR cur IN (SELECT sequence_name FROM USER_SEQUENCES WHERE sequence_name = 'PERSON_SEQ') LOOP
    EXECUTE IMMEDIATE 'DROP SEQUENCE person_seq';
  END LOOP;

  EXECUTE IMMEDIATE 'CREATE SEQUENCE person_seq START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';

  EXECUTE IMMEDIATE 'CREATE TABLE Person (
					   Id NUMBER PRIMARY KEY,
					   FirstName VARCHAR2(50),
					   LastName VARCHAR2(50),
					   DateCreated DATE,
					   Active VARCHAR(1))';

  EXECUTE IMMEDIATE 'CREATE or REPLACE TRIGGER trg#person#b_ins
                     BEFORE INSERT ON Person FOR EACH ROW
                     BEGIN
                       SELECT person_seq.nextval INTO :NEW.id  FROM dual;
                     END;';
END;

