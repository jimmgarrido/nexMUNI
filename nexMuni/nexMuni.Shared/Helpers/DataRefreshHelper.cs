using nexMuni.DataModels;
using SQLite;
using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace nexMuni.Helpers
{
    public class DataRefreshHelper
    {
        public List<Route> routesList;
        public Dictionary<string, Stop> stopDictionary;

        public async Task RefreshDataAsync() 
        {
            routesList = new List<Route>();
            //var allStopsList = new List<Stop>(1050);
            stopDictionary = new Dictionary<string, Stop>(1100);
            //var baseUrl = "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r=";


            //Get XML listing all routes of SF Muni
            //var reader = XmlReader.Create("http://webservices.nextbus.com/service/publicXMLFeed?command=routeList&a=sf-muni");

            //var routesString = await GetDataString("http://webservices.nextbus.com/service/publicXMLFeed?command=routeList&a=sf-muni");
            //var routeDoc = XDocument.Parse(routesString);

            //var routeElements = routeDoc.Element("body").Elements("route");

            //Read each XML line and add the route number or letter to a list
            //foreach (var routeElement in routeElements)
            //{
            //    var title = routeElement.Attribute("title").Value;
            //    var num = routeElement.Attribute("tag").Value;
            //    routesList.Add(new Route(title, num));
            //}

            var routeData = await GetDataString("http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni");
            var routeDataDoc = XDocument.Parse(routeData);
            //var routeDataElements = routeDataDoc.Element("body").Elements("route"); 
            var routeDataElements = routeDataDoc.Element("body").Elements();

            foreach (var route in routeDataElements)
            {
                var title = route.Attribute("title").Value;
                var num = route.Attribute("tag").Value;
                routesList.Add(new Route(title, num));

                foreach (var stop in route.Elements("stop"))
                {
                    //var name = stop.Attribute("title").Value;
                    var stopTag = string.Join("|", num, stop.Attribute("tag").Value);
                    var lon = double.Parse(stop.Attribute("lon").Value);
                    var lat = double.Parse(stop.Attribute("lat").Value);
                    var routeNum = num;

                    var key = stop.Attribute("title").Value;

                    //Remove "Inbound/Outbound" from metro station names
                    if (key.Contains("Outbound"))
                    {
                        key = key.Remove(key.IndexOf("Outbound") - 1);
                        //stop.StopName = key;
                    }
                    else if (key.Contains("Inbound"))
                    {
                        key = key.Remove(key.IndexOf("Inbound") - 1);
                        //stop.StopName = key;
                    }
                    else if (key.Contains("OB") || key.Contains("IB"))
                    {
                        key = key.Remove(key.Length - 2);
                        //stop.StopName = key;
                    }

                    //if (allStopsList.Exists(x => x.StopName == key))
                    //{
                    //    var matchedStop = allStopsList.Find(x => x.StopName == key);
                    //    matchedStop.StopTags = string.Join(",", matchedStop.StopTags, stopTag);

                    //    if (!matchedStop.Routes.Contains(routeNum))
                    //    {
                    //        matchedStop.Routes = string.Join(", ", matchedStop.Routes, routeNum);
                    //    }
                    //}
                    //else
                    //{
                    //    allStopsList.Add(new Stop(key, stopTag, num, lat, lon));
                    //}

                    if (stopDictionary.ContainsKey(key))
                    {
                        var matchedStop = stopDictionary[key];
                        matchedStop.StopTags = string.Join(",", matchedStop.StopTags, stopTag);

                        if (!matchedStop.Routes.Contains(routeNum))
                        {
                            matchedStop.Routes = string.Join(", ", matchedStop.Routes, routeNum);
                        }
                    }
                    else
                    {
                        stopDictionary.Add(key,new Stop(key, stopTag, num, lat, lon));
                    }
                }
            }

            string[] splitName;
            string reversedName = string.Empty;

            ////List method
            //for (int i = 0; i < allStopsList.Count; i++)
            //{
            //    var stop = allStopsList[i];
            //    splitName = stop.StopName.Split(new string[] { " & " }, StringSplitOptions.None);

            //    if (splitName.Length > 1)
            //    {
            //        reversedName = string.Join(" ", splitName[1], "&", splitName[0]);
            //    }

            //    if (allStopsList.Exists(x => x.StopName == reversedName))
            //    {
            //        var matchedStop = allStopsList.Find(x => x.StopName == reversedName);
            //        stop.StopTags = string.Join(",", stop.StopTags, matchedStop.StopTags);

            //        allStopsList.Remove(matchedStop);
            //    }
            //}

            for (int i = 0; i < stopDictionary.Count; i++)
            {
                var stop = stopDictionary.Values.ElementAt(i);
                splitName = stop.StopName.Split(new string[] { " & " }, StringSplitOptions.None);

                if (splitName.Length > 1)
                {
                    reversedName = string.Join(" ", splitName[1], "&", splitName[0]);
                }

                if (stopDictionary.ContainsKey(reversedName))
                {
                    var matchedStop = stopDictionary[reversedName];
                    stop.StopTags = string.Join(",", stop.StopTags, matchedStop.StopTags);

                    stopDictionary.Remove(reversedName);
                }
            }
        }

        private async Task<string> GetDataString(string url)
        {
            string response = string.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip");
                client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("deflate");

                try
                {
                    response = await client.GetStringAsync(new Uri(url));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return response;
        }

    }
}
