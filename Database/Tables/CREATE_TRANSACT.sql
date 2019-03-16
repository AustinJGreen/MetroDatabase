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