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
        internal static string[] models =
        {
            "Gillig Phantom",
            "New Flyer D40LF",
            "New Flyer DE60LF",
            "New Flyer DE60LF",
            "New Flyer DE60LFA",
            "Orion VII",
            "New Flyer DE60LFR",
            "New Flyer Xcelsior XDE35",
            "New Flyer Xcelsior XDE40",
            "New Flyer Xcelsior XT40",
            "New Flyer Xcelsior XDE60",
            "New Flyer Xcelsior XT60",
            "Proterra Catalyst",
            "Gillig Low Floor"
        };

        internal static int[] seats =
        {
            25,
            30,
            40,
            40,
            45,
            40,
            50,
            40,
            40,
            60,
            60,
            60,
            60,
            60
        };

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

            bool multipleRoutes = false;

            BusStop[] stops = new BusStop[stopNames.Length];
            for (int i = 0; i < stopNames.Length; i++)
            {
                if (string.Compare(stopNames[i], "route", true) == 0)
                {
                    multipleRoutes = true;
                    continue;
                }

                //Parse individual stop
                //int lastDash = stopNames[i].LastIndexOf('-')
                string[] stopArgs = stopNames[i].Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
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
                if (multipleRoutes)
                {
                    string routeNameStr = times[0];
                    if (string.Compare(routeNameStr, route.RouteName) != 0)
                    {
                        continue;
                    }
                }

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
            List<string> common = new List<string>();
            common.Add("north");
            common.Add("south");
            common.Add("west");
            common.Add("east");
            common.Add("valley");

            BusStop[] sarr = new BusStop[stops.Count];
            double[] distances = new double[stops.Count];
            for (int i = 0; i < stops.Count; i++)
            {
                string[] streets = stops[i].CrossStreet.Split('&');

                double score = 0.5 * Levenshtein.Compute(pr.Address, stops[i].CrossStreet) +
                            0.25 * Levenshtein.Compute(pr.Name, stops[i].Name) +
                            Levenshtein.Compute(pr.City, stops[i].Name) +
                            Levenshtein.Compute(pr.Address, stops[i].Name);
                distances[i] = score;
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

            int max = parkAndRides.Max(t => t.Address.Length);

            Console.WriteLine("Done.");

            //Get routes so we can match to closest stop
            List<BusStop> stops = new List<BusStop>();
            using (MetroDB db = new MetroDB("guest", "guest"))
            {
                if (db.Connect())
                {
                    var tableData = db.GetTable("BUS_STOP");
                    for (int i = 0; i < tableData[0].Count; i++)
                    {
                        stops.Add(new BusStop(tableData[1][i], tableData[2][i], int.Parse(tableData[0][i])));
                    }
                }
                else
                {
                    Console.WriteLine("Unable to retrieve bus stop data. Cannot generate p&r data.");
                    return;
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
            Random rng = new Random();

            int busCount = 1540;
            List<int> availableNumbers = Enumerable.Range(1, 9998).ToList();

            List<BusRoute> routes = new List<BusRoute>();
            List<string> driverIds = null;
            List<string> baseAddresses = null;
            using (MetroDB db = new MetroDB("guest", "guest"))
            {
                if (db.Connect())
                {
                    var routeData = db.GetTable("BUS_ROUTE");
                    for (int i = 0; i < routeData[0].Count; i++)
                    {
                        BusRoute br = new BusRoute();
                        br.ToName = routeData[0][i];
                        br.FromName = routeData[1][i];
                        br.RouteNumber = int.Parse(routeData[2][i]);
                        br.RouteName = routeData[2][i];
                        br.DaysOfOperation = byte.Parse(routeData[3][i]);
                        routes.Add(br);
                    }

                    var driverDat = db.GetTable("DRIVER");
                    driverIds = driverDat[0];

                    var basesDat = db.GetTable("BASE");
                    baseAddresses = basesDat[0];
                }
                else
                {
                    Console.WriteLine("Unable to retrieve data. Cannot generate bus data.");
                    return;
                }
            }

            StringBuilder bldr = new StringBuilder();
            List<BusRoute> distro = new List<BusRoute>(routes);
            for (int i = 0; i < busCount; i++)
            {
                int bIndex = rng.Next(availableNumbers.Count);
                int bNum = availableNumbers[bIndex];
                availableNumbers.RemoveAt(bIndex);

                int dIndex = rng.Next(driverIds.Count);
                string dId = driverIds[dIndex];
                driverIds.RemoveAt(dIndex);
                
                int rIndex = rng.Next(distro.Count);
                BusRoute route = distro[rIndex];
                distro.RemoveAt(rIndex);
                if (distro.Count == 0)
                {
                    distro.AddRange(routes);
                }

                int mIndex = rng.Next(models.Length);
                string model = models[rng.Next(mIndex)];
                int seatCnt = seats[mIndex];

                double rngNorm = rng.NextNormal(0, 1);
                int milesDriven = (int)((3 + rngNorm) * ((models.Length - mIndex) * 25000));

                int baIndex = rng.Next(baseAddresses.Count);
                string bAddr = baseAddresses[baIndex];


                bldr.AppendFormat("INSERT INTO BUS Values ({0}, {1}, \"{2}\", \"{3}\", {4}, \"{5}\", {6}, {7}, \"{8}\");\r\n",
                    bNum,
                    dId,
                    route.ToName,
                    route.FromName,
                    route.RouteNumber,
                    model,
                    seatCnt,
                    milesDriven,
                    bAddr);
            }

            string queries = bldr.ToString();
            string curDir = Environment.CurrentDirectory;
            File.WriteAllText(Path.Combine(curDir, "buses.sql"), queries);
        }

        internal static void GenerateTransactions()
        {
            Random rng = new Random();
            List<string> busNums = null;
            using (MetroDB db = new MetroDB("guest", "guest"))
            {
                if (db.Connect())
                {
                    var tableData = db.GetTable("BUS");
                    busNums = tableData[0];
                }
                else
                {
                    Console.WriteLine("Unable to retrieve bus stop data. Cannot generate transit card data.");
                    return;
                }
            }

            List<int> tids = Enumerable.Range(10000, 89999).ToList();
            StringBuilder bldr = new StringBuilder();
            for (int i = 0; i < 40000; i++)
            {
                int tIndex = rng.Next(tids.Count);
                int tId = tids[tIndex];

                int bIndex = rng.Next(busNums.Count);
                string bNum = busNums[bIndex];

                string type = rng.Next(2) == 0 ? "cash" : "card";
                double dollarAmount = 2.75;
                if (rng.NextDouble() < 0.25 && type.Equals("cash")) //25% of the time dont pay exact
                {
                    switch (rng.Next(10))
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            dollarAmount = 5;
                            break;
                        case 6:
                        case 7:
                        case 8:
                            dollarAmount = 10;
                            break;
                        case 9:
                            dollarAmount = 20;
                            break;
                    }
                }
                else
                {
                    switch (rng.Next(5))
                    {
                        case 0:
                            dollarAmount = 1;
                            break;
                        case 1:
                            dollarAmount = 1.50;
                            break;
                        case 2:
                        case 3:
                        case 4:
                            dollarAmount = 2.75;
                            break;
                    }
                }

                bldr.AppendFormat("INSERT INTO TRANSACT Values ({0}, \"{1}\", {2}, {3});\r\n", tId, type, dollarAmount, bNum);
            }

            string queries = bldr.ToString();
            string curDir = Environment.CurrentDirectory;
            File.WriteAllText(Path.Combine(curDir, "transactions.sql"), queries);
        }

        internal static void GenerateTransitCard()
        {
            StringBuilder bldr = new StringBuilder();
            for (int i = 0; i < 5000; i++)
            {

            }

            string queries = bldr.ToString();
            string curDir = Environment.CurrentDirectory;
            File.WriteAllText(Path.Combine(curDir, "parkandrides.sql"), queries);
        }

        internal static void Main(string[] args)
        {
            //GenerateDriverInserts();
            GenerateTransactions();
        }
    }
}
