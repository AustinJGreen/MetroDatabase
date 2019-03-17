using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MetroRouteScraper
{
    internal class Program
    {
        internal static string GetParameter(string src, string start, string end)
        {
            int startIndex = src.IndexOf(start);
            if (startIndex == -1)
            {
                return null;
            }

            string paramStart = src.Substring(startIndex + start.Length);
            int endIndex = paramStart.IndexOf(end);
            if (endIndex == -1)
            {
                return null;
            }

            return paramStart.Substring(0, endIndex);
        }

        internal static string GetRouteTableCSV(string routeNum, bool from)
        {
            HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(string.Format("https://kingcounty.gov/~/media/depts/transportation/metro/data/schedules-09-22-2018/{0}0{1}.csv", from ? 'a' : 'b', routeNum));
            wreq.Method = "GET";

            try
            {
                using (HttpWebResponse wres = (HttpWebResponse)wreq.GetResponse())
                using (StreamReader rdr = new StreamReader(wres.GetResponseStream()))
                {
                    return rdr.ReadToEnd();
                }
            }
            catch
            {
                return null;
            }
        }

        internal static BusRouteStop[] GetBusStops(BusRoute route, string csv)
        {
            if (csv == null)
            {
                return null;
            }

            string[] lines = csv.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] stopNames = lines[0].Split(',');

            BusStop[] stops = new BusStop[stopNames.Length];
            for (int i = 0; i < stopNames.Length; i++)
            {
                //Parse individual stop
                string[] stopArgs = stopNames[i].Split('-');
                if (stopArgs.Length < 2)
                {
                    continue;
                }
                
                string crossSt = HttpUtility.HtmlDecode(stopArgs[stopArgs.Length > 2 ? 1 : 0].Trim('*', ' '));
                string stopName = HttpUtility.HtmlDecode(stopArgs.Length > 2 ? stopArgs[0].Trim('*', ' ') : crossSt);
                string stopNumStr = stopArgs[stopArgs.Length - 1].Trim('*', ' ');

                int stopNum = -1;
                if (stopNumStr.StartsWith("Stop #"))
                {
                    int.TryParse(stopNumStr.Substring(6), out stopNum);
                }

                BusStop stop = new BusStop(stopName, crossSt, stopNum);
                stops[i] = stop;
            }

            BusRouteStop[] routeStops = new BusRouteStop[(lines.Length - 1) * stops.Length];
            for (int i = 1; i < lines.Length; i++)
            {
                //string routeTimes
                string[] times = lines[i].Split(',');
                for (int t = 0; t < times.Length; t++)
                {
                    string time = times[t].Length > 8 ? times[t].Substring(0, times[t].LastIndexOf(' ')) : times[t];
                    time = time.Replace("\'B\'", "").Replace("\'AB\'", "").Replace("\'H\'", "").Trim('\'').Trim(' ');
                    if (time.Contains("---") || time.Length > 8 || time.Length < 4)
                    {
                        continue;
                    }

                    DateTime? date = null;
                    try
                    {
                        date = DateTime.ParseExact(time, "h:mm tt", CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        try
                        {
                            date = DateTime.ParseExact(time, "h:mm", CultureInfo.InvariantCulture);
                        }
                        catch
                        {

                        }
                    }

                    if (date.HasValue)
                    {
                        TimeSpan eta = date.Value.TimeOfDay;
                        BusRouteStop routeStop = new BusRouteStop(route, stops[t], eta, t);
                        routeStops[(i - 1) * times.Length + t] = routeStop;
                    }
                }
            }

            return routeStops;
        }

        internal static string StripHTML(string input)
        {
            if (input == null)
            {
                return null;
            }

            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        internal static BusRoute[] GetKingCountyRoute(HtmlWeb webLoader, Uri uri)
        {
            HtmlDocument routeDoc = webLoader.Load(uri);
            if (uri.AbsoluteUri.EndsWith(".pdf"))
            {
                return new BusRoute[0];
            }

            HtmlNode routeNode = routeDoc.DocumentNode;

            string routeNumber = HttpUtility.HtmlDecode(StripHTML(GetParameter(routeNode.OuterHtml, "var routeNumber=\'", "\'")));
            string routeName = HttpUtility.HtmlDecode(StripHTML(GetParameter(routeNode.OuterHtml, "var routeName=\'", "\'")))?.TrimStart('0');
            string aDestShort = HttpUtility.HtmlDecode(StripHTML(GetParameter(routeNode.OuterHtml, "var aDestShort=\'", "\'")))?.Replace("To ", "").Trim();
            string bDestShort = HttpUtility.HtmlDecode(StripHTML(GetParameter(routeNode.OuterHtml, "var bDestShort=\'", "\'")))?.Replace("To ", "").Trim();

            HtmlNode daysNode = routeNode.SelectSingleNode("//*[@id=\"schedule_wrapper\"]/ul[1]");
            if (daysNode == null)
            {
                return new BusRoute[0];
            }

            byte daysOfOperation = 0b1111100;
            bool onlyWeekday = routeNode.OuterHtml.Contains("/* Hiding Saturday and Sunday buttons */");
            if (!onlyWeekday)
            {
                daysOfOperation = 0b1111111;
            }

            int routeId = -1;
            if (!int.TryParse(routeNumber, out routeId))
            {
                return new BusRoute[0];
            }

            BusRoute toRoute = new BusRoute();
            toRoute.RouteName = routeName;
            toRoute.RouteNumber = routeId;
            toRoute.FromName = aDestShort;
            toRoute.ToName = bDestShort;
            toRoute.DaysOfOperation = daysOfOperation;

            string toCsv = GetRouteTableCSV(routeNumber, true);
            BusRouteStop[] routeStops = GetBusStops(toRoute, toCsv);
            toRoute.Stops = routeStops;

            BusRoute fromRoute = new BusRoute();
            fromRoute.RouteName = routeName;
            fromRoute.RouteNumber = routeId;
            fromRoute.FromName = bDestShort;
            fromRoute.ToName = aDestShort;
            fromRoute.DaysOfOperation = daysOfOperation;

            string fromCsv = GetRouteTableCSV(routeNumber, false);
            BusRouteStop[] fromStopData = GetBusStops(fromRoute, fromCsv);
            fromRoute.Stops = fromStopData;

            return new BusRoute[]
            {
                toRoute,
                fromRoute
            };
        }

        internal static BusRoute[] GetSoundTransitRoute(HtmlWeb webLoader, Uri uri)
        {
            return new BusRoute[0];
        }

        internal static BusRoute[] GetRoute(HtmlWeb webLoader, Uri uri)
        {
            switch (uri.Host)
            {
                case "kingcounty.gov":
                    return GetKingCountyRoute(webLoader, uri);
                case "soundtransit.org":
                    return GetSoundTransitRoute(webLoader, uri);
            }

            return new BusRoute[0];
        }

        internal static List<BusRoute> ScrapeRoutes()
        {
            HtmlWeb webLoader = new HtmlWeb();
            HtmlDocument doc = webLoader.Load("https://kingcounty.gov/depts/transportation/metro.aspx");

            List<BusRoute> routes = new List<BusRoute>();

            var routeHeadNode = doc.DocumentNode.SelectSingleNode("//*[@id=\"route_select2_2\"]");
            if (routeHeadNode == null)
            {
                return routes;
            }

            foreach (HtmlNode node in routeHeadNode.ChildNodes.Skip(2).Where(t => t.Name == "option"))
            {
                string routeName = node.InnerText;
                string relativeUrl = node.GetAttributeValue("value", null);
                Uri parsedUri = null;
                Uri.TryCreate(relativeUrl, UriKind.RelativeOrAbsolute, out parsedUri);
                if (!parsedUri.IsAbsoluteUri)
                {
                    parsedUri = new Uri(new Uri("https://kingcounty.gov"), parsedUri);
                }

                Console.WriteLine("Parsing {0}", routeName);
                routes.AddRange(GetRoute(webLoader, parsedUri));
            }

            return routes;
        }

        internal static List<ParkAndRide> ScrapeParkAndRides()
        {
            List<ParkAndRide> parkAndRides = new List<ParkAndRide>();

            HtmlWeb webLoader = new HtmlWeb();
            HtmlDocument doc = webLoader.Load("https://www.wsdot.wa.gov/Choices/parkride.htm");

            var prNode = doc.DocumentNode.SelectSingleNode("//*[@id=\"block-system-main\"]/div/div/div/table[25]/tbody");
            string city = null;
            foreach(HtmlNode node in prNode.ChildNodes.Skip(1))
            {
                int startIndex = 0;
                if (string.Compare(node.ChildNodes[0].GetAttributeValue("align", null), "left") == 0)
                {
                    city = node.ChildNodes[0].InnerText;
                    startIndex += 2;
                }

                if (node.ChildNodes.Count >= 4)
                {
                    string name = node.ChildNodes[startIndex].InnerText;
                    string address = node.ChildNodes[startIndex + 2].InnerText;
                    string parkingSpacesStr = node.ChildNodes[startIndex + 4].InnerText;
                    int parkingSpaces = -1;
                    if (!int.TryParse(parkingSpacesStr, out parkingSpaces))
                    {
                        continue;
                    }

                    ParkAndRide parkAndRide = new ParkAndRide(city, address, name, parkingSpaces);
                    parkAndRides.Add(parkAndRide);
                }

            }

            return parkAndRides;
        }

        internal static void RemoveInvalidRoutes(List<BusRoute> routes)
        {
            for (int i = routes.Count - 1; i >= 0; i--)
            {
                if (routes[i].Stops == null || routes[i].Stops.Length == 0 || string.IsNullOrEmpty(routes[i].FromName) || string.IsNullOrEmpty(routes[i].ToName))
                {
                    routes.RemoveAt(i);
                }
            }
        }

        internal static string GenerateRouteInserts(List<BusRoute> routes)
        {
            //Get list of stops 
            List<BusStop> stops = new List<BusStop>();
            List<int> availableIds = Enumerable.Range(10000, 89999).ToList();
            for (int i = 0; i < routes.Count; i++)
            {
                for (int j = 0; j < routes[i].Stops.Length; j++)
                {
                    if (routes[i].Stops[j] != null)
                    {
                        BusStop curStop = routes[i].Stops[j].Stop;
                        if (curStop != null && !stops.Contains(curStop))
                        {
                            stops.Add(curStop);
                            if (curStop.StopID != -1)
                            {
                                availableIds.Remove(curStop.StopID);
                            }
                        }
                    }
                }
            }

            //Give stops without an ID an ID.
            Random rng = new Random();
            for (int j = 0; j < stops.Count; j++)
            {
                if (stops[j].StopID == -1)
                {
                    int index = rng.Next(availableIds.Count);
                    int uniqueId = availableIds[index];
                    availableIds.RemoveAt(index);
                    stops[j].StopID = uniqueId;
                }
            }

            StringBuilder bldr = new StringBuilder();
            bldr.AppendLine("-- Insert routes");
            for (int i = 0; i < routes.Count; i++)
            {
                bldr.AppendFormat("INSERT INTO BUS_ROUTE Values (\"{0}\", \"{1}\", \"{2}\", {3});\r\n",
                    routes[i].ToName,
                    routes[i].FromName,
                    routes[i].RouteNumber,
                    routes[i].DaysOfOperation
                    );
            }

            bldr.AppendLine();
            bldr.AppendLine("-- Insert stops");
            for (int i = 0; i < stops.Count; i++)
            {
                bldr.AppendFormat("INSERT INTO BUS_STOP Values ({0}, \"{1}\", \"{2}\");\r\n",
                    stops[i].StopID,
                    stops[i].Name,
                    stops[i].CrossStreet
                    );
            }

            bldr.AppendLine();
            bldr.AppendLine("-- Insert route stops");
            HashSet<string> routesInserted = new HashSet<string>();
            for (int i = 0; i < routes.Count; i++)
            {
                for (int j = 0; j < routes[i].Stops.Length; j++)
                {
                    BusRoute route = routes[i];
                    BusRouteStop routeStop = route?.Stops[j];
                    BusStop stop = routeStop?.Stop;
                    if (routeStop != null && stop != null)
                    {
                        if (stop.StopID == -1)
                        {
                            stop = stops.Find(t => t.Equals(stop));
                        }

                        if (stop != null)
                        {
                            string routeStr = string.Format("INSERT INTO BUS_ROUTE_STOPS Values (\"{0}\", \"{1}\", \"{2}\", {3}, \"{4}\", {5});",
                                route.FromName,
                                route.ToName,
                                route.RouteNumber,
                                stop.StopID,
                                new DateTime(routeStop.ETA.Ticks).ToString("hh:mm:sss"),
                                routeStop.StopNumber
                                );
                            if (!routesInserted.Contains(routeStr))
                            {
                                routesInserted.Add(routeStr);
                                bldr.AppendLine(routeStr);
                            }
                        }
                    }
                }
            }

            return bldr.ToString();
        }

        internal static string GenerateParkAndRideInserts(List<ParkAndRide> parkandrides)
        {
            StringBuilder bldr = new StringBuilder();
            bldr.AppendLine("-- Insert park and rides");
            for (int i = 0; i < parkandrides.Count; i++)
            {
                bldr.AppendFormat("INSERT INTO PARK_AND_RIDE Values (\"{0}\", \"{1}\", \"{2}\", {3}, {4});\r\n",
                    parkandrides[i].City,
                    parkandrides[i].Address,
                    parkandrides[i].Name,
                    parkandrides[i].ParkingSpots,
                    parkandrides[i].StopID
                    );
            }

            return bldr.ToString();
        }

        internal static void GenerateRouteInserts()
        {
            List<BusRoute> routes = ScrapeRoutes();
            RemoveInvalidRoutes(routes);
            routes = routes.Distinct().ToList();

            //int maxFrom = routes.Max(t => t.FromName.Length);
            //int maxTo = routes.Max(t => t.ToName.Length);

            string queries = GenerateRouteInserts(routes);
            string curDir = Environment.CurrentDirectory;
            File.WriteAllText(Path.Combine(curDir, "routes.sql"), queries);
        }

        internal static BusStop ClosestStop(ParkAndRide pr, List<BusStop> stops)
        {
            BusStop[] sarr = new BusStop[stops.Count];
            int[] distances = new int[stops.Count];
            for (int i = 0; i < stops.Count; i++)
            {
                string[] streets = stops[i].CrossStreet.Split('&');

                int best = Levenshtein.Compute(pr.Address, stops[i].CrossStreet);
                if (streets.Length == 2)
                {
                    int dis1 = Levenshtein.Compute(pr.Address, streets[0]);
                    int dis2 = Levenshtein.Compute(pr.Address, streets[1]);
                    int avg = (dis1 + dis2) / 2;
                    best = Math.Min(avg, best);
                }

                distances[i] = best;
                sarr[i] = stops[i];
            }

            Array.Sort(distances, sarr);
            return sarr[0];
        }

        internal static void GenerateParkAndRideInserts()
        {
            //Get park and rides
            Console.Write("Scraping park and rides...");
            List<ParkAndRide> parkAndRides = ScrapeParkAndRides();
            Console.WriteLine("Done.");

            //Get routes so we can match to closest stop
            List<BusRoute> routes = ScrapeRoutes();
            RemoveInvalidRoutes(routes);
            routes = routes.Distinct().ToList();

            //Get list of stops 
            List<BusStop> stops = new List<BusStop>();
            List<int> availableIds = Enumerable.Range(10000, 89999).ToList();
            for (int i = 0; i < routes.Count; i++)
            {
                for (int j = 0; j < routes[i].Stops.Length; j++)
                {
                    if (routes[i].Stops[j] != null)
                    {
                        BusStop curStop = routes[i].Stops[j].Stop;
                        if (curStop != null && !stops.Contains(curStop))
                        {
                            stops.Add(curStop);
                            if (curStop.StopID != -1)
                            {
                                availableIds.Remove(curStop.StopID);
                            }
                        }
                    }
                }
            }

            //Give stops without an ID an ID.
            Random rng = new Random();
            for (int j = 0; j < stops.Count; j++)
            {
                if (stops[j].StopID == -1)
                {
                    int index = rng.Next(availableIds.Count);
                    int uniqueId = availableIds[index];
                    availableIds.RemoveAt(index);
                    stops[j].StopID = uniqueId;
                }
            }

            //Match to closest stop
            Console.WriteLine("Matching Stops...");
            foreach (var p in parkAndRides)
            {
                Console.WriteLine("Searching for stop at {0}", p.Name);
                BusStop closest = ClosestStop(p, stops);
                p.StopID = closest.StopID;
            }

            string queries = GenerateParkAndRideInserts(parkAndRides);
            string curDir = Environment.CurrentDirectory;
            File.WriteAllText(Path.Combine(curDir, "parkandrides.sql"), queries);
        }

        internal static void GenerateEmployeeInserts()
        {
            int employees = 2716;
            Random rng = new Random();
            RandomName rngName = new RandomName(rng);
            StringBuilder bldr = new StringBuilder();

            List<int> idsAvailable = Enumerable.Range(1, 9989).ToList();
            for (int i = 0; i < employees; i++)
            {
                int idIndex = rng.Next(idsAvailable.Count);
                int id = idsAvailable[idIndex];
                idsAvailable.RemoveAt(idIndex);
                int ssn1 = rng.Next(100, 1000);
                int ssn2 = rng.Next(10, 100);
                int ssn3 = rng.Next(1000, 10000);
                string name = rngName.Generate(rng.Next(2) == 0 ? Sex.Female : Sex.Male);
                bldr.AppendFormat("INSERT INTO EMPLOYEE VALUES ({0}, \"{1}-{2}-{3}\", \"{4}\");\r\n", id, ssn1, ssn2, ssn3, name);
            }

            string queries = bldr.ToString();
            string curDir = Environment.CurrentDirectory;
            File.WriteAllText(Path.Combine(curDir, "employees.sql"), queries);
        }

        

        internal static void GenerateDriverInserts()
        {
            Random rng = new Random();
            int drivers = rng.Next(2000, 2100);

            List<string> employeeIds = null;
            using (MetroDB db = new MetroDB("guest", "guest"))
            {
                if (db.Connect())
                {
                    var employeeData = db.GetTable("EMPLOYEE");
                    employeeIds = employeeData[0];
                }
                else
                {
                    Console.WriteLine("Unable to retrieve employee data. Cannot generate driver data.");
                    return;
                }
            }

            List<int> driverIds = Enumerable.Range(1, 9998).ToList();

            StringBuilder bldr = new StringBuilder();
            for (int i = 0; i < drivers; i++)
            {
                int eIndex = rng.Next(employeeIds.Count);
                string eId = employeeIds[eIndex];
                employeeIds.RemoveAt(eIndex);

                int dIndex = rng.Next(driverIds.Count);
                int dId = driverIds[dIndex];
                driverIds.RemoveAt(dIndex);

                double rngNorm = rng.NextNormal(0, 1);
                int milesDriven = (int)((3 + rngNorm) * 250000);
                bldr.AppendFormat("INSERT INTO DRIVER Values ({0}, {1}, {2});\r\n", eId, dId, milesDriven);
            }

            string queries = bldr.ToString();
            string curDir = Environment.CurrentDirectory;
            File.WriteAllText(Path.Combine(curDir, "drivers.sql"), queries);
        }

        internal static void GenerateBusInserts()
        {
            int busCount = 1540;
            List<int> availableNumbers = Enumerable.Range(1, 9998).ToList();

            
        }

        internal static void Main(string[] args)
        {
            GenerateDriverInserts();
        }
    }
}
