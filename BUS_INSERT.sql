
--BUS
--fail cases
INSERT INTO BUS VALUES (-1, 1000000, 'To Place1', 'From Place1', '123451','old_model1', 10, 0, 'baseaddress1');
INSERT INTO BUS VALUES (1, 100000, 'To Place2', 'From Place2', '123452','old_model2', 10, 0, 'baseaddress2');
INSERT INTO BUS VALUES (1, 1000000, 'To Place3', 'From Place3', '123453','old_model3', 9, 0, 'baseaddress3');
INSERT INTO BUS VALUES (1, 1000000, 'To Place4', 'From Place4', '123454','old_model4', 10, -1, 'baseaddress4');
INSERT INTO BUS VALUES (99999991, 9999999, 'To Place5', 'From Place5', '123455','old_model5', 100, 12345, 'baseaddress5');
INSERT INTO BUS VALUES (9999999, 99999991, 'To Place6', 'From Place6', '123456','old_model6', 100, 12345, 'baseaddress6');
INSERT INTO BUS VALUES (9999999, 9999999, 'To Place7', 'From Place7', '123457','old_model7', 1001, 12345, 'baseaddress7');
INSERT INTO BUS VALUES (9999999, 9999999, 'To Place8', 'From Place8', '123458','old_model8', 100, 12345, 'baseaddress8');

--min
INSERT INTO BUS VALUES (1, 1000000, 'To Place9', 'From Place9', '123459','old_model9', 10, 0, 'baseaddress9');

--max
INSERT INTO BUS VALUES (9999999, 9999999, 'To Place11', 'From Place11', '1234511','old_model11', 100, 12345, 'baseaddress11');

