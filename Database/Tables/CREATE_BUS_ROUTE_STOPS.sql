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