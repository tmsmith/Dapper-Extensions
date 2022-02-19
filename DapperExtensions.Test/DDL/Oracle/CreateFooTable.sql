﻿BEGIN
  FOR cur IN (SELECT table_name FROM USER_TABLES WHERE table_name = 'FOOTABLE') LOOP
    EXECUTE IMMEDIATE 'DROP TABLE FOOTABLE CASCADE CONSTRAINTS';
  END LOOP;

  FOR cur IN (SELECT sequence_name FROM USER_SEQUENCES WHERE sequence_name = 'FOOTABLE_SEQ') LOOP
    EXECUTE IMMEDIATE 'DROP SEQUENCE FOOTABLE_SEQ';
  END LOOP;

  EXECUTE IMMEDIATE 'CREATE SEQUENCE FOOTABLE_SEQ START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';

  EXECUTE IMMEDIATE 'CREATE TABLE FOOTABLE (
                       Id NUMBER PRIMARY KEY,
                       FirstName VARCHAR2(50),
                       LastName VARCHAR2(50),
                       DateOfBirth DATE)';

  EXECUTE IMMEDIATE 'CREATE or REPLACE TRIGGER trg#FOOTABLE#b_ins
                     BEFORE INSERT ON FOOTABLE FOR EACH ROW
                     BEGIN
                       SELECT FOOTABLE_SEQ.nextval INTO :NEW.id  FROM dual;
                     END;';
END;
