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