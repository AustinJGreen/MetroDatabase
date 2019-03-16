namespace MetroRouteScraper
{
    public class ParkAndRide
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string Name { get; set; }
        public int ParkingSpots { get; set; }
        public int StopID { get; set; }

        public ParkAndRide(string city, string address, string name, int parkingSpots)
        {
            City = city;
            Address = address;
            Name = name;
            ParkingSpots = parkingSpots;
        }
    }
}
