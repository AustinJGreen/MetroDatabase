CREATE TABLE DRIVER(
    Employee_ID INT UNIQUE,
    Driver_ID INT UNIQUE,
    Miles_Driven INT,
    PRIMARY KEY(Employee_ID, Driver_ID),
    CONSTRAINT FK_DRIVER_E_ID FOREIGN KEY(Employee_ID) REFERENCES EMPLOYEE(ID),
    CONSTRAINT DRIVER_E_ID_range CHECK(Employee_ID >= 1000000 AND Employee_ID <= 9999999),
    CONSTRAINT DRIVER__ID_range CHECK(Driver_ID >= 1000000 AND Driver_ID <= 9999999),
    CONSTRAINT DRIVER_Mile_min CHECK(Miles_Driven >= 0)    
);