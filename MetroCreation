DROP TABLE EMPLOYEE;
DROP TABLE DRIVER;
DROP TABLE BUS;
DROP TABLE BUS_ROUTE;
DROP TABLE BASE;
DROP TABLE BUS_ROUTE_STOPS;
DROP TABLE PARK_AND_RIDE;
DROP TABLE TRANS;
DROP TABLE TRANSIT_CARD;

--Employee TABLE
CREATE TABLE EMPLOYEE(
    ID INT NOT NULL,
    Ssn VARCHAR(10) NOT NULL,
    E_Name VARCHAR (30) NOT NULL,


    PRIMARY KEY(ID),
    CONSTRAINT ID_range CHECK(ID >= 0),
    CONSTRAINT Ssn_range CHECK(length(Ssn) = 10),
    CONSTRAINT Name_range CHECK(length(E_Name) <= 3 AND length(E_Name) <= 30)
);

--Driver TABLE
CREATE TABLE DRIVER(
    Employee_ID INT NOT NULL,
    Driver_ID INT NOT NULL,
    Miles_Driven INT NOT NULL,
    PRIMARY KEY(Employee_ID,Driver_ID),
    FOREIGN KEY(Employee_ID) REFERENCES EMPLOYEE(ID),
    CONSTRAINT E_ID_range CHECK(Employee_ID >= 1000000 AND Employee_ID <= 9999999),
    CONSTRAINT D_ID_range CHECK(Driver_ID >= 1000000 AND Driver_ID <= 9999999),
    CONSTRAINT Mile_min CHECK(Miles_Driven >= 0)    
);

--Bus TABLE
CREATE TABLE BUS(
    Bus_number INT NOT NULL,
    Driver_ID INT NOT NULL,
    To_name VARCHAR(30) NULL,
    From_name VARCHAR(30) NULL,
    Route_number VARCHAR(30) NULL,
    Bus_model VARCHAR(30) NULL,
    Seats_available INT NOT NULL,
    Miles INT NOT NULL,
    Base_address VARCHAR(30),

    PRIMARY KEY(Bus_number, Driver_ID, Base_address),
    FOREIGN KEY(Driver_ID) REFERENCES DRIVER(Driver_ID),
    FOREIGN KEY(Base_address) REFERENCES BASE(Base_address),

    CONSTRAINT Bus_range CHECK (Bus_number >= 0 AND Bus_number <= 9999999),
    CONSTRAINT D_ID_range CHECK(Driver_ID >= 1000000 AND Driver_ID <= 9999999),
    CONSTRAINT To_range CHECK(length(To_name) >= 3 AND length(To_name) <= 30),
    CONSTRAINT From_range CHECK(length(From_name) >= 3 AND length(From_name) <=30),
    CONSTRAINT Route_range CHECK(length(Route_number) >= 3 AND length(Route_number) <= 30),
    CONSTRAINT Model_range CHECK(length(Bus_model) >=3 AND length(Bus_model) <= 30),
    CONSTRAINT Seats_range CHECK(Seats_available >= 10 AND Seats_available <=100),
    CONSTRAINT Mile_range CHECK(Miles >= 0)    
);

--Bus Route Table
CREATE TABLE BUS_ROUTE(
    To_name VARCHAR(30) NULL,
    From_name VARCHAR(30) NULL,
    Route_number VARCHAR(30) NULL,
    Days_of_operation VARCHAR(2) DEFAULT 'mo',
    PRIMARY KEY(To_name, From_name, Route_number),


    CONSTRAINT To_range CHECK(length(To_name) >= 3 AND length(To_name) <= 30),
    CONSTRAINT From_range CHECK(length(From_name) >= 3 AND length(From_name) <=30),
    CONSTRAINT Route_range CHECK(length(Route_number) >= 3 AND length(Route_number) <= 30),
    CONSTRAINT Days_range CHECK(Days_of_operation IN('mo', 'tu', 'we', 'th', 'fr', 'sa', 'su'))
);

--Base Table
CREATE TABLE BASE(
    Base_address VARCHAR(30) NOT NULL,
    Base_name VARCHAR(30) NOT NULL,
    Parking_spots INT NOT NULL,
    PRIMARY KEY(Base_address, Base_name),
    CONSTRAINT address_range CHECK(length(Base_address) >= 3 AND length(Base_address) <= 30),
    CONSTRAINT name_range CHECK(length(Base_name) >= 3 AND length(Base_name) <= 30),
    CONSTRAINT parking_range CHECK(Parking_spots >= 0)
);

--Bus Stop Table
CREATE TABLE BUS_STOP(
    Stop_ID INT NOT NULL,
    Stop_Name VARCHAR(30) NOT NULL,
    Cross_Street VARCHAR(30) NOT NULL,
    PRIMARY KEY(Stop_ID),
    CONSTRAINT id_range CHECK(Stop_ID >= 0),
    CONSTRAINT name_range CHECK(length(Stop_Name) >= 3 AND length(Stop_Name) <= 30),
    CONSTRAINT cross_range CHECK(length(Cross_Street) >= 3 AND length(Cross_Street) <= 30)
);

--Bus Route Stop TABLE
CREATE TABLE BUS_ROUTE_STOPS(
    To_name VARCHAR(30) NULL,
    From_name VARCHAR(30) NULL,
    Route_number VARCHAR(30) NULL,
    Stop_ID INT NOT NULL,
    ETA DECIMAL NOT NULL,
    Stop_number INT NOT NULL,

    PRIMARY KEY(To_name, From_name, Route_number, Stop_ID),
    FOREIGN KEY(To_name) REFERENCES BUS_ROUTE(To_name),
    FOREIGN KEY(From_name) REFERENCES BUS_ROUTE(From_name),
    FOREIGN KEY(Route_number) REFERENCES BUS_ROUTE(Route_number),
    FOREIGN KEY(Stop_ID) REFERENCES BUS_STOP(Stop_ID),

    CONSTRAINT To_range CHECK(length(To_name) >= 3 AND length(To_name) <= 30),
    CONSTRAINT From_range CHECK(length(From_name) >= 3 AND length(From_name) <=30),
    CONSTRAINT Route_range CHECK(length(Route_number) >= 3 AND length(Route_number) <= 30),
    CONSTRAINT id_range CHECK(Stop_ID >= 0),
    CONSTRAINT eta_range CHECK(ETA >= 0 AND ETA <= 360),
    CONSTRAINT stop_num_range CHECK(Stop_number >= 1 AND Stop_number <= 50)
);


--PARK and RIDE TABLE
CREATE TABLE PARK_AND_RIDE(
    Park_Address VARCHAR(30) NOT NULL,
    Park_Name VARCHAR(30) NOT NULL,
    Park_Spots INT NOT NULL,
    Stop_ID INT NULL,
    
    PRIMARY KEY(Park_Address, Park_Name, Stop_ID),
    FOREIGN KEY(Stop_ID) REFERENCES BUS_STOP(Stop_ID),

    CONSTRAINT stop_range CHECK(STOP_ID >= 0),
    CONSTRAINT park_add_range CHECK(length(Park_Address) >= 3 AND length(Park_Address) <= 30),
    CONSTRAINT park_name_range CHECK(length(Park_Name) >= 3 AND length(Park_Name) <= 30),
    CONSTRAINT park_spot_range CHECK(Park_Spots > 0 AND Park_Address < 100000)
);

--TRANSACT TABLE
CREATE TABLE TRANSACT(
    Transaction_ID INT NOT NULL,
    Transaction_Type VARCHAR(30) NOT NULL,
    Dollar_amount DECIMAL NOT NULL,
    Bus_number INT NOT NULL,
    PRIMARY KEY(Transaction_ID, Bus_number),
    FOREIGN KEY(Bus_number) REFERENCES BUS(Bus_number),

    CONSTRAINT id_range CHECK(Transaction_ID >= 0),
    CONSTRAINT type_range CHECK(Transaction_Type IN('cash', 'card')),
    CONSTRAINT money_range CHECK(Dollar_amount >= 0),
    CONSTRAINT Bus_range CHECK(Bus_number > 0 AND Bus_number <= 999)
);

--TRANSIT CARD TABLE
CREATE TABLE TRANSIT_CARD(
    Transaction_ID INT NOT NULL,
    Card_number CHAR(16) NOT NULL,
    Has_disability BOOLEAN NOT NULL,
    Balance DECIMAL NOT NULL,
    Account_name VARCHAR(30),

    PRIMARY KEY(Transaction_ID, Card_number),
    FOREIGN KEY(Transaction_ID) REFERENCES TRANSACT(Transaction_ID),

    CONSTRAINT id_range CHECK(Transaction_ID >= 0),
    CONSTRAINT Card_range CHECK(length(Card_number) = 16),
    CONSTRAINT Name_range CHECK( length(Account_name) >= 3 AND length(Account_name) <= 30)




);
