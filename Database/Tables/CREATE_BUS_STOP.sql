CREATE TABLE BUS_STOP(
    Stop_ID INT UNIQUE,
    Stop_Name VARCHAR(60) NOT NULL,
    Cross_Street VARCHAR(60) NOT NULL,
    PRIMARY KEY(Stop_ID),
    CONSTRAINT bstop_id_range CHECK(Stop_ID >= 0),
    CONSTRAINT bstop_name_range CHECK(LEN(Stop_Name) >= 3 AND LEN(Stop_Name) <= 30),
    CONSTRAINT bstop_cross_range CHECK(LEN(Cross_Street) >= 3 AND LEN(Cross_Street) <= 30)
);