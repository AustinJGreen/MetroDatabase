--Queries
/*
When will my bus arrive?
People want to know the estimated time the bus they need to take arrives because they are on a tight schedule and need to plan ahead of time. 

How do I get from A to B?
People want to know what routes help them get to where they need to go.
The City wants to know what routes are available to the people



Where does route A stop?
The City may need to know where routes cycle and how to efficiently string routes together for busses.

What times does route A stop at stop B?
People want to have an accurate assessment of how traversing by bus will be
The City may be able to improve or adjust this as needed to improve city traffic flow.

How many people boarded route A today?
People may want to plan around how congested each bus may be
The City may want to plan their routes and bus distribution on how many are using certain routes or buses.

How many people boarding are disabled?
The City may want to know the number of disabled using transportation, and how to improve their situation .

What is the most profitable route?
The City needs to make a profit on the transportation system, and this will help clarify what routes are the most productive

How many buses can park at base A?
The City may need to know this to resolve any congestion at bus stops for different routes.

How many employees need to drive today?
The City and its Employees may need this information to control bus distribution and the number of jobs in public transportation.

*/

--Where does route A stop?
--Need BUS_ROUTE, BUS_ROUTE_STOPS, BUS_STOP
SELECT BS.Stop_Name
FROM BUS_ROUTE AS BR, BUS_STOP AS BS, BUS_ROUTE_STOPS AS BRS
WHERE BRS.Stop_ID = BS.Stop_ID AND
BRS.Route_number = BR.Route_number AND
BR.Route_number = 'A'
GROUP BY BRS.Stop_name;

--How do I get from point A to point B (to/from names)
--outputting to and from for a bus_route
SELECT BR.Route_number
FROM BUS_ROUTE AS BR
WHERE BR.To_name = 'A' AND
BR.From_name = 'B';


--How do I get from point A to point B (to/from stops)
SELECT BS.Route_number
FROM BUS_ROUTE AS BR, BUS_STOP AS BS, BUS_ROUTE_STOPS AS BRS
WHERE BRS.Stop_ID = BS.Stop_ID AND
BRS.Route_number = BR.Route_number AND
BS.Stop_Name = 'A' OR
BS.Stop_Name = 'B';

--When will my bus arrive?
--get ETA of current bus_route_stops
--need to provide route number?
SELECT BRS.ETA
FROM BUS_ROUTE AS BR, BUS_STOP AS BS, BUS_ROUTE_STOPS AS BRS
WHERE BRS.Stop_ID = BS.Stop_ID AND
BRS.Route_number = BR.Route_number AND
BRS.Route_number = 'MyRoute';

--How many buses can park at base A?
SELECT Parking_spots
FROM BASE
WHERE Base_name = 'A'




