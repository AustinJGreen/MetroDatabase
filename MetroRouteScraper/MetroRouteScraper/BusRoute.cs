namespace MetroRouteScraper
{
    public class BusRoute
    {
        public int RouteNumber { get; set; }
        public string RouteName { get; set; }
        public string ToName { get; set; }
        public string FromName { get; set; }
        public byte DaysOfOperation { get; set; }
        public BusRouteStop[] Stops { get; set; }
    }
}
