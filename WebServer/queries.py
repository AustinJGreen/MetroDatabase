#db.execute_command("SELECT * FROM BASE")
from metrodb import MetroDB
db = MetroDB('guest', 'guest')
db.connect()
print("Connected!")


#INSERT INTO BUS_ROUTE Values ("Federal Way TC", "Tukwila International Blvd Link Station", "671", 127);
A = '671'
print("Where does route " + str(A)  + " stop?")
baseTable = db.execute_command("SELECT BS.Stop_Name FROM BUS_ROUTE AS BR, BUS_STOP AS BS, BUS_ROUTE_STOPS AS BRS WHERE BRS.Stop_ID = BS.Stop_ID AND BRS.Route_number = BR.Route_number AND BR.Route_number = \'" + str(A) +"\' GROUP BY BRS.Stop_name;")
print(baseTable.fetch_row(maxrows=0))

A = 'University District'
B = 'Wallingford'
print("How do I get from " + A + "to " + B)
baseTable = db.execute_command("SELECT BS.Route_number FROM BUS_ROUTE AS BR, BUS_STOP AS BS, BUS_ROUTE_STOPS AS BRS WHERE BRS.Stop_ID = BS.Stop_ID AND BRS.Route_number = BR.Route_number AND BS.Stop_Name = \'" + A +  "\' OR BS.Stop_Name = \'" + B "\';"
)
print(baseTable.fetch_row(maxrows=0))

db.close()


