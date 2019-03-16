CREATE TABLE BASE(
    Base_address VARCHAR(50) NOT NULL,
    Base_name VARCHAR(20) NOT NULL,
    Parking_spots INT,
    PRIMARY KEY(Base_address, Base_name),
    CONSTRAINT base_address_range CHECK(LEN(Base_address) >= 3 AND LEN(Base_address) <= 30),
    CONSTRAINT base_name_range CHECK(LEN(Base_name) >= 3 AND LEN(Base_name) <= 30),
    CONSTRAINT base_parking_range CHECK(Parking_spots >= 0)
);