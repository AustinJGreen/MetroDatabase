using System;

namespace MetroRouteScraper
{
    public class BusRouteStop
    {
        public BusRoute Route { get; set; }
        public BusStop Stop { get; set; }
        public TimeSpan ETA { get; set; }
        public int StopNumber { get; set; }

        public BusRouteStop(BusRoute route, BusStop stop, TimeSpan eta, int stopNumber)
        {
            Route = route;
            Stop = stop;
            ETA = eta;
            StopNumber = stopNumber;
        }
    }
}
