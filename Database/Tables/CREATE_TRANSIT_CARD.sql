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