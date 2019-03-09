﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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

        internal static void RemoveInvalidRoutes(List<BusRoute> routes)
        {
            for (int i = routes.Count - 1; i >= 0; i--)
            {
                if (routes[i].Stops == null || routes[i].Stops.Length == 0)
                {
                    routes.RemoveAt(i);
                }
            }
        }

        internal static string GenerateQueries(List<BusRoute> routes)
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
                            bldr.AppendFormat("INSERT INTO BUS_ROUTE_STOPS Values (\"{0}\", \"{1}\", \"{2}\", {3}, \"{4}\", {5});\r\n",
                                route.FromName,
                                route.ToName,
                                route.RouteNumber,
                                stop.StopID,
                                new DateTime(routeStop.ETA.Ticks).ToString("h:mm tt"),
                                routeStop.StopNumber
                                );
                        }
                    }
                }
            }

            return bldr.ToString();
        }

        internal static void Main(string[] args)
        {
            List<BusRoute> routes = ScrapeRoutes();
            RemoveInvalidRoutes(routes);

            string queries = GenerateQueries(routes);
            File.WriteAllText(@"C:\Users\austi\Desktop\queries.sql", queries);
        }
    }
}