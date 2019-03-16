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