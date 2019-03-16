CREATE TABLE BUS(
    Bus_number INT,
    Driver_ID INT,
    To_name VARCHAR(60) NULL,
    From_name VARCHAR(60) NULL,
    Route_number VARCHAR(30) NULL,
    Bus_model VARCHAR(30) NULL,
    Seats_available INT,
    Miles INT,
    Base_address VARCHAR(30),

    PRIMARY KEY(Bus_number, Driver_ID, Base_address),
    CONSTRAINT FK_Bus_Driver_ID FOREIGN KEY(Driver_ID) REFERENCES DRIVER(Driver_ID),
    CONSTRAINT FK_Bus_Base_address FOREIGN KEY(Base_address) REFERENCES BASE(Base_address),

    CONSTRAINT BUS_range CHECK (Bus_number >= 0 AND Bus_number <= 9999999),
    CONSTRAINT BUS_D_ID_range CHECK(Driver_ID >= 1000000 AND Driver_ID <= 9999999),
    CONSTRAINT BUS_To_range CHECK(LEN(To_name) >= 3 AND LEN(To_name) <= 30),
    CONSTRAINT BUS_From_range CHECK(LEN(From_name) >= 3 AND LEN(From_name) <=30),
    CONSTRAINT BUS_Route_range CHECK(LEN(Route_number) >= 3 AND LEN(Route_number) <= 30),
    CONSTRAINT BUS_Model_range CHECK(LEN(Bus_model) >=3 AND LEN(Bus_model) <= 30),
    CONSTRAINT BUS_Seats_range CHECK(Seats_available >= 10 AND Seats_available <=100),
    CONSTRAINT BUS_Mile_range CHECK(Miles >= 0) 
);