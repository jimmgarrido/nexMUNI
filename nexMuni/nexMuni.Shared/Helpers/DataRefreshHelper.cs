using nexMuni.DataModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Web.Http;

namespace nexMuni.Helpers
{
    public class DataRefreshHelper
    {
        public async Task RefreshDataAsync()
        {
            var routesList = new List<Route>();
            var allStopsList = new List<Stop>();
            var baseUrl = "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r=";


            //Get XML listing all routes of SF Muni
            //var reader = XmlReader.Create("http://webservices.nextbus.com/service/publicXMLFeed?command=routeList&a=sf-muni");

            //var routesString = await GetData("http://webservices.nextbus.com/service/publicXMLFeed?command=routeList&a=sf-muni");
            //var routeDoc = XDocument.Parse(routesString);

            //var routeElements = routeDoc.Element("body").Elements("route");

            ////Read each XML line and add the route number or letter to a list
            //foreach (var routeElement in routeElements)
            //{
            //    var title = routeElement.Attribute("title").Value;
            //    var num = routeElement.Attribute("tag").Value;
            //    routesList.Add(new Route(title, num));
            //}

            //Go through each route in RouteNum and append to the base URL. Get XML from each URL and parse
            //foreach(var route in routesList)
            //{
            //    var stopsUrl = string.Join(null, baseUrl, route.RouteNumber);
            //    var stopsString = await GetData(stopsUrl);

            //    var stopsDoc = XDocument.Parse(stopsString);
            //    var stopElements = stopsDoc.Element("body").Element("route").Elements("stop");

            //    foreach(var element in stopElements)
            //    {
            //var name = element.Attribute("title").Value;
            //var stopTag = string.Join("|", route.RouteNumber, element.Attribute("tag").Value);
            //var lon = double.Parse(element.Attribute("lon").Value);
            //var lat = double.Parse(element.Attribute("lat").Value);
            //var num = route.RouteNumber;

            //allStopsList.Add(new Stop(name, stopTag, num, lat, lon));
            //    }
            //}

            var routeData = await GetData("http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni");
            var routeDataDoc = XDocument.Parse(routeData);
            var routeDataElements = routeDataDoc.Element("body").Elements("route");

            foreach(var route in routeDataElements)
            {
                var title = route.Attribute("title").Value;
                var num = route.Attribute("tag").Value;
                routesList.Add(new Route(title, num));

                foreach(var stop in route.Elements("stop"))
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

                    if (allStopsList.Exists(x => x.StopName == key))
                    {
                        var matchedStop = allStopsList.Find(x => x.StopName == key);
                        matchedStop.StopTags = string.Join(",", matchedStop.StopTags, stopTag);

                        if (!matchedStop.Routes.Contains(routeNum))
                        {
                            matchedStop.Routes = string.Join(", ", matchedStop.Routes, routeNum);
                        }
                    }
                    else
                    {
                        allStopsList.Add(new Stop(key, stopTag, num, lat, lon));
                    }
                }
            }

            //Prepare to sort all the stops for duplicates
            //var key = string.Empty;
            //var sortedList = new List<Stop>();

            //foreach(var stop in allStopsList)
            //{
            //    key = stop.StopName;

            //    //Remove "Inbound/Outbound" from metro station names
            //    if (key.Contains("Outbound"))
            //    {
            //        key = key.Remove(key.IndexOf("Outbound") - 1);
            //        stop.StopName = key;
            //    }
            //    else if (key.Contains("Inbound"))
            //    {
            //        key = key.Remove(key.IndexOf("Inbound") - 1);
            //        stop.StopName = key;
            //    }
            //    else if (key.Contains("OB") || key.Contains("IB"))
            //    {
            //        key = key.Remove(key.Length - 2);
            //        stop.StopName = key;
            //    }

            //    if (sortedList.Exists(x => x.StopName == key))
            //    {
            //        var matchedStop = sortedList.Find(x => x.StopName == key);
            //        matchedStop.StopTags = string.Join(",", matchedStop.StopTags, stop.StopTags);

            //        if(!matchedStop.Routes.Contains(stop.Routes))
            //        {
            //            matchedStop.Routes = string.Join(", ", matchedStop.Routes, stop.Routes);
            //        }
            //    }
            //    else
            //    {
            //        sortedList.Add(stop);
            //    }
            //}


            //Prepare for second sort to remove similar bus stops e.g. 5th & Mission St / Mission St. & 5th St
            //string[] splitName;
            //string reversedName = string.Empty;

            //for(int i=0; i<sortedList.Count; i++)
            //{
            //    var stop = sortedList[i];
            //    splitName = stop.StopName.Split(new string[] { " & " }, StringSplitOptions.None);

            //    if(splitName.Length > 1)
            //    {
            //        reversedName = string.Join(" ", splitName[1], "&", splitName[0]);
            //    }

            //    if(sortedList.Exists(x => x.StopName == reversedName))
            //    {
            //        var matchedStop = sortedList.Find(x => x.StopName == reversedName);
            //        stop.StopTags = string.Join(",", stop.StopTags, matchedStop.StopTags);

            //        sortedList.Remove(matchedStop);
            //    }
            //}

            string[] splitName;
            string reversedName = string.Empty;

            for (int i = 0; i < allStopsList.Count; i++)
            {
                var stop = allStopsList[i];
                splitName = stop.StopName.Split(new string[] { " & " }, StringSplitOptions.None);

                if (splitName.Length > 1)
                {
                    reversedName = string.Join(" ", splitName[1], "&", splitName[0]);
                }

                if (allStopsList.Exists(x => x.StopName == reversedName))
                {
                    var matchedStop = allStopsList.Find(x => x.StopName == reversedName);
                    stop.StopTags = string.Join(",", stop.StopTags, matchedStop.StopTags);

                    allStopsList.Remove(matchedStop);
                }
            }


            await ApplicationData.Current.LocalFolder.CreateFileAsync("refresh.sqlite",CreationCollisionOption.ReplaceExisting);
            var refreshDb = await ApplicationData.Current.LocalFolder.GetFileAsync("refresh.sqlite");

            var refreshDbPath = refreshDb.Path;
            var _refreshAsyncConnection = new SQLiteAsyncConnection(refreshDbPath);
            //var _favoritesAsyncConnection = new SQLiteAsyncConnection(() => new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(favoriteDbPath, false)));

            await _refreshAsyncConnection.CreateTableAsync<Stop>();
            foreach(var stop in allStopsList)
            {
                await _refreshAsyncConnection.InsertAsync(stop);
            }

            await _refreshAsyncConnection.CreateTableAsync<Route>();
            foreach (var route in routesList)
            {
                await _refreshAsyncConnection.InsertAsync(route);
            }

            try
            {
                //StorageFile muniDb = await ApplicationData.Current.LocalFolder.GetFileAsync("muni.sqlite");

                StorageFile dbFile = await ApplicationData.Current.LocalFolder.GetFileAsync("refresh.sqlite");
                await dbFile.CopyAsync(ApplicationData.Current.LocalFolder, "muni.sqlite", NameCollisionOption.ReplaceExisting);

                //StorageFile muniDb = await ApplicationData.Current.LocalFolder.GetFileAsync("muni.sqlite");

                //var messageBox = new MessageDialog(message);
                //await messageBox.ShowAsync();
            }
            catch
            {
                
            }
        }

        private async Task<string> GetData(string url)
        {
            string response = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip");
                client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("deflate");

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
