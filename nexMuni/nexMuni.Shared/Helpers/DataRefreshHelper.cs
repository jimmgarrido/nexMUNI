using nexMuni.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Web.Http;

namespace nexMuni.Helpers
{
    public class DataRefreshHelper
    {
        public async Task RefreshDataAsync()
        {
            var routeNums = new List<string>();
            var routeTitles = new List<string>();
            var allStopsList = new List<Stop>();
            var baseUrl = "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r=";


            //Get XML listing all routes of SF Muni
            //XmlTextReader reader = new XmlTextReader("http://webservices.nextbus.com/service/publicXMLFeed?command=routeList&a=sf-muni");

            var routesString = await GetData("http://webservices.nextbus.com/service/publicXMLFeed?command=routeList&a=sf-muni");
            var routeDoc = XDocument.Parse(routesString);
            
            var routeElements = routeDoc.Element("body").Elements("route");

            //Read each XML line and add the route number or letter to a list
            foreach (var routeElement in routeElements)
            {
                routeNums.Add(routeElement.Attribute("tag").Value);
                routeTitles.Add(routeElement.Attribute("title").Value);
            }

            //while (reader.Read())
            //{
            //    switch (reader.NodeType)
            //    {
            //        case XmlNodeType.Element:
            //            if (reader.Name == "body") break;
            //            reader.MoveToFirstAttribute();
            //            routeTags.Add(reader.Value);
            //            reader.MoveToNextAttribute();
            //            routeTitles.Add(reader.Value);
            //            break;
            //        default:
            //            break;
            //    }
            //}

            //String routes = "routes.sqlite";
            //String stops = "stops.sqlite";
            //String db = "Databases";

            //if (!Directory.Exists(db))
            //    Directory.CreateDirectory(db);
            //Directory.SetCurrentDirectory(db);

            //if (File.Exists(routes))
            //    File.Delete(routes);

            //if (File.Exists(stops))
            //    File.Delete(stops);

            //Create db of routes
            //if (!singleFile)
            //{
            //    Console.WriteLine("\nWriting to routes database...");

            //    var routesDb = new SQLiteConnection(routes);
            //    routesDb.CreateTable<RouteData>();
            //    for (int i = 0; i < routeTitles.Count; i++)
            //    {
            //        routesDb.Insert(new RouteData(routeTitles[i]));
            //    }
            //    Console.WriteLine("---routes.sqlite created---");
            //}

            string temp;
            int counter = 0;

            //Go through each route in RouteNum and append to the base URL. Get XML from each URL and parse
            foreach(var num in routeNums)
            {
                var stopsUrl = string.Join(null, baseUrl, num);
                var stopsString = await GetData(stopsUrl);

                var stopsDoc = XDocument.Parse(stopsString);
                var stopElements = stopsDoc.Element("route").Elements("stop");

                foreach(var element in stopElements)
                {
                    var name = element.Attribute("title").Value;
                    var stopTag = string.Join("|", num, element.Attribute("tag").Value);
                    var lon = double.Parse(element.Attribute("lon").Value);
                    var lat = double.Parse(element.Attribute("lat").Value);

                    allStopsList.Add(new Stop(name, stopTag, lat, lon));
                }
            }

            //for (int i = 0; i < routeTags.Count; i++)
            //{
            //    temp = baseUrl + routeTags[i];
            //    reader = new XmlTextReader(temp);

            //    //Parse XML of all stops for a specific route
            //    while (reader.Read())
            //    {
            //        switch (reader.NodeType)
            //        {
            //            case XmlNodeType.Element:
            //                if (reader.Name == "body") break;
            //                else if (reader.Name == "route") break;
            //                else if (reader.Name == "direction") break;
            //                else if (reader.Name == "path") break;
            //                else if (reader.Name == "point") break;

            //                //Add stop data to the allStopsList List
            //                while (reader.MoveToNextAttribute())
            //                {
            //                    if (reader.Name == "tag")
            //                    {
            //                        allStopsList.Add(new Stop(reader.Value, routeTags[i], 0));
            //                        counter++;
            //                    }
            //                    if (reader.Name == "title")
            //                    {
            //                        allStopsList[counter - 1].title = reader.Value;
            //                    }
            //                    if (reader.Name == "lon")
            //                    {
            //                        allStopsList[counter - 1].lon = reader.Value;
            //                    }
            //                    if (reader.Name == "lat")
            //                    {
            //                        allStopsList[counter - 1].lat = reader.Value;
            //                    }
            //                }
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}

            //Prepare to sort all the stops for duplicates and output results to a text file
            var key = string.Empty;
            var sortedList = new List<Stop>();

            foreach(var stop in allStopsList)
            {
                key = stop.StopName;

                //Remove "Inbound/Outbound" from metro station names
                if (key.Contains("Outbound"))
                {
                    key = key.Remove(key.IndexOf("Outbound") - 1);
                }
                else if (key.Contains("Inbound"))
                {
                    key = key.Remove(key.IndexOf("Inbound") - 1);
                }
                else if (key.Contains("OB") || key.Contains("IB"))
                {
                    key = key.Remove(key.Length - 2);
                }

                if (sortedList.Exists(x => x.StopName == key))
                {
                    var matchedStop = sortedList.Find(x => x.StopName == key);
                    matchedStop.StopTags = string.Join(",", matchedStop.StopTags, stop.StopTags);

                    if(!matchedStop.Routes.Contains(stop.Routes))
                    {
                        matchedStop.Routes = string.Join(", ", matchedStop.Routes, stop.Routes);
                    }
                }
                else
                {
                    sortedList.Add(stop);
                }
            }

            //counter = 0;
            //for (int j = 0; j < allStopsList.Count; j++)
            //{
            //    key = allStopsList[j].title;
            //    if (key != null)
            //    {
            //        if (!(firstSort.Exists(y => y.title == key)))
            //        {
            //            found = allStopsList.FindAll(x => x.title == key);
            //            firstSort.Add(new Stop(found[0].tag, found[0].route, 1));
            //            firstSort[counter].title = found[0].title;
            //            firstSort[counter].lon = found[0].lon;
            //            firstSort[counter].lat = found[0].lat;

            //            for (int l = 1; l < found.Count; l++)
            //            {
            //                if (firstSort[counter].allRoutes.Contains(" " + found[l].route) == false)
            //                {
            //                    firstSort[counter].AddRoute(found[l].route);
            //                    firstSort[counter].AddTag(found[l].route, found[l].tag);
            //                    firstSort[counter].AddStopTag(found[l].tag);
            //                }
            //                if (firstSort[counter].allRoutes.Contains(" " + found[l].route) == true)
            //                {
            //                    if (firstSort[counter].allTags.Contains(found[l].route + "|" + found[l].tag) != true)
            //                    {
            //                        firstSort[counter].AddTag(found[l].route, found[l].tag);
            //                        firstSort[counter].AddStopTag(found[l].tag);
            //                    }
            //                }
            //            }
            //            counter++;
            //        }
            //    }
            //}

            //Remove "Inbound/Outbound" from metro station names
            //foreach (Stop s in firstSort)
            //{
            //    if (s.title.Contains("Inbound"))
            //    {
            //        s.title = s.title.Replace(" Inbound", "");
            //    }
            //    if (s.title.Contains("Outbound"))
            //    {
            //        s.title = s.title.Replace(" Outbound", "");
            //    }
            //}

            //Prepare for second sort to remove similar bus stops e.g. 5th & Mission St / Mission St. & 5th St
            string[] splitName;
            string reversedName = string.Empty;

            foreach (var stop in sortedList)
            {
                splitName = stop.StopName.Split(new string[] { " & " }, StringSplitOptions.None);
                
                if(splitName.Length > 1)
                {
                    reversedName = string.Join(" ", splitName[1], "&", splitName[0]);
                }

                if(sortedList.Exists(x => x.StopName == reversedName))
                {
                    var matchedStop = sortedList.Find(x => x.StopName == reversedName);
                    stop.StopTags = string.Join(",", stop.StopTags, matchedStop.StopTags);

                    sortedList.Remove(matchedStop);
                }
            }

            //found.Clear();
            //used.Clear();

            //foreach (Stop s in sortedList)
            //{
            //    titleSplit = s.title.Split('&');

            //    if (!used.Contains(s.title))
            //    {
            //        if (titleSplit.Count() > 1) key = titleSplit[1].Substring(1) + " & " + titleSplit[0].Substring(0, (titleSplit[0].Length - 1));
            //        else key = s.title;

            //        if (sortedList.Any(x => x.title == key))
            //        {
            //            found = sortedList.FindAll(y => y.title == key);

            //            foreach (Stop d in found)
            //            {
            //                foreach (string r in d.allRoutes)
            //                {
            //                    s.AddRoute(r);
            //                }

            //                foreach (string t in d.allTags)
            //                {
            //                    s.AddTag(t);
            //                }

            //                foreach (string t in d.StopTags)
            //                {
            //                    s.AddStopTag(t);
            //                }
            //            }
            //        }
            //        used.Add(key);
            //        sortedList.Add(s);
            //    }
            //}

            //Create db of stops
            //if (!singleFile)
            //{
            //    var stopsDb = new SQLiteConnection(stops);
            //    stopsDb.CreateTable<StopData>();

            //    for (int k = 0; k < sortedList.Count; k++)
            //    {
            //        stopsDb.Insert(new StopData(sortedList[k].title, Double.Parse(sortedList[k].lon), Double.Parse(sortedList[k].lat), sortedList[k].ListRoutes(), sortedList[k].ListTags()));
            //    }
            //}
        }

        private async Task<string> GetData(string url)
        {
            string response = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    response = await client.GetStringAsync(new Uri(url));
                }
                catch (Exception ex)
                {

                }
            }

            return response;
        }
    }
}
