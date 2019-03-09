namespace MetroRouteScraper
{
    public class BusStop
    {
        public int StopID { get; set; }
        public string Name { get; set; }
        public string CrossStreet { get; set; }

        public BusStop(string name, string crossSt, int stopId)
        {
            Name = name;
            CrossStreet = crossSt;
            StopID = stopId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return StopID.GetHashCode() + Name.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is BusStop)
            {
                BusStop stop = obj as BusStop;
                return string.Compare(Name, stop.Name, true) == 0 ||
                    string.Compare(CrossStreet, stop.CrossStreet, false) == 0 ||
                    StopID == stop.StopID;
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("Stop #{0}, {1}", StopID, CrossStreet);
        }
    }
}
