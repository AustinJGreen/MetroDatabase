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

        public override int GetHashCode()
        {
            unchecked
            {
                int sum = 0;
                sum += FromName.GetHashCode();
                sum += ToName.GetHashCode();
                sum += RouteName.GetHashCode();
                return sum;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is BusRoute)
            {
                BusRoute br = obj as BusRoute;
                return string.Compare(br.FromName, FromName) == 0 &&
                       string.Compare(br.ToName, ToName) == 0 &&
                       string.Compare(br.RouteName, RouteName) == 0;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", FromName, ToName, RouteName);
        }
    }
}
