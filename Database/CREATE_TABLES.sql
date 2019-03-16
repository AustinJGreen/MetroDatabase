CREATE TABLE EMPLOYEE(
    ID INT UNIQUE,
    Ssn VARCHAR(10) NOT NULL,
    E_Name VARCHAR (30) NOT NULL,

    PRIMARY KEY(ID),
    CONSTRAINT ID_range CHECK(ID >= 0),
    CONSTRAINT Ssn_range CHECK(LEN(Ssn) = 10),
    CONSTRAINT Name_range CHECK(LEN(E_Name) <= 3 AND LEN(E_Name) <= 30)
);

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

CREATE TABLE BASE(
    Base_address VARCHAR(30) NOT NULL,
    Base_name VARCHAR(30) NOT NULL,
    Parking_spots INT,
    PRIMARY KEY(Base_address, Base_name),
    CONSTRAINT base_address_range CHECK(LEN(Base_address) >= 3 AND LEN(Base_address) <= 30),
    CONSTRAINT base_name_range CHECK(LEN(Base_name) >= 3 AND LEN(Base_name) <= 30),
    CONSTRAINT base_parking_range CHECK(Parking_spots >= 0)
);

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

CREATE TABLE BUS_ROUTE(
    To_name VARCHAR(60) NOT NULL,
    From_name VARCHAR(60) NOT NULL,
    Route_number VARCHAR(30) NOT NULL,
    Days_of_operation INT,
    
    PRIMARY KEY(To_name, From_name, Route_number),
    CONSTRAINT br_To_range CHECK(LEN(To_name) >= 3 AND LEN(To_name) <= 30),
    CONSTRAINT br_From_range CHECK(LEN(From_name) >= 3 AND LEN(From_name) <= 30),
    CONSTRAINT br_Route_range CHECK(LEN(Route_number) >= 3 AND LEN(Route_number) <= 30),
    CONSTRAINT br_Route_doo CHECK(Days_of_operation >= 0 AND Days_of_operation <= 127)
);

CREATE TABLE BUS_STOP(
    Stop_ID INT UNIQUE,
    Stop_Name VARCHAR(60) NOT NULL,
    Cross_Street VARCHAR(60) NOT NULL,
    PRIMARY KEY(Stop_ID),
    CONSTRAINT bstop_id_range CHECK(Stop_ID >= 0),
    CONSTRAINT bstop_name_range CHECK(LEN(Stop_Name) >= 3 AND LEN(Stop_Name) <= 30),
    CONSTRAINT bstop_cross_range CHECK(LEN(Cross_Street) >= 3 AND LEN(Cross_Street) <= 30)
);

CREATE TABLE BUS_ROUTE_STOPS(
    To_name VARCHAR(60) NOT NULL,
    From_name VARCHAR(60) NOT NULL,
    Route_number VARCHAR(30) NOT NULL,
    Stop_ID INT,
    ETA TIME,
    Stop_number INT,

	PRIMARY KEY(To_name, From_name, Route_number, Stop_ID, ETA, Stop_number),
	CONSTRAINT BRS_FK_ROUTE FOREIGN KEY(To_name, From_name, Route_number) REFERENCES BUS_ROUTE(To_name, From_name, Route_number),
    CONSTRAINT BRS_FK_STOP FOREIGN KEY(Stop_ID) REFERENCES BUS_STOP(Stop_ID),
    
    CONSTRAINT brs_To_range CHECK(LEN(To_name) >= 3 AND LEN(To_name) <= 30),
    CONSTRAINT brs_From_range CHECK(LEN(From_name) >= 3 AND LEN(From_name) <=30),
    CONSTRAINT brs_Route_range CHECK(LEN(Route_number) >= 3 AND LEN(Route_number) <= 30),
    CONSTRAINT brs_id_range CHECK(Stop_ID >= 0),
    CONSTRAINT brs_stop_num_range CHECK(Stop_number >= 1 AND Stop_number <= 50)
);

CREATE TABLE TRANSACT(
    Transaction_ID INT,
    Transaction_Type VARCHAR(30) NOT NULL,
    Dollar_amount DECIMAL,
    Bus_number INT,
    PRIMARY KEY(Transaction_ID, Bus_number),
    FOREIGN KEY(Bus_number) REFERENCES BUS(Bus_number),

    CONSTRAINT id_range CHECK(Transaction_ID >= 0),
    CONSTRAINT type_range CHECK(Transaction_Type IN('cash', 'card')),
    CONSTRAINT money_range CHECK(Dollar_amount >= 0),
    CONSTRAINT Bus_range CHECK(Bus_number > 0 AND Bus_number <= 999)
);

CREATE TABLE PARK_AND_RIDE(
    Park_Address VARCHAR(30) NOT NULL,
    Park_Name VARCHAR(30) NOT NULL,
    Park_Spots INT,
    Stop_ID INT,
    
    PRIMARY KEY(Park_Address, Park_Name, Stop_ID),
    FOREIGN KEY(Stop_ID) REFERENCES BUS_STOP(Stop_ID),

    CONSTRAINT pr_stop_range CHECK(STOP_ID >= 0),
    CONSTRAINT park_add_range CHECK(LEN(Park_Address) >= 3 AND LEN(Park_Address) <= 30),
    CONSTRAINT park_name_range CHECK(LEN(Park_Name) >= 3 AND LEN(Park_Name) <= 30),
    CONSTRAINT park_spot_range CHECK(Park_Spots > 0 AND Park_Address < 100000)
);

CREATE TABLE TRANSIT_CARD(
    Transaction_ID INT,
    Card_number CHAR(16) NOT NULL,
    Has_disability BIT,
    Balance DECIMAL,
    Account_name VARCHAR(30),

    PRIMARY KEY(Transaction_ID, Card_number),
    FOREIGN KEY(Transaction_ID) REFERENCES TRANSACT(Transaction_ID),

    CONSTRAINT id_range CHECK(Transaction_ID >= 0),
    CONSTRAINT Name_range CHECK(LEN(Account_name) >= 3 AND LEN(Account_name) <= 30)
);