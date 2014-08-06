using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Windows.Devices.Geolocation;

namespace nexMuni
{
    class SearchModel
    {
        public static bool IsDataLoaded { get; set; }
        public static ObservableCollection<string> RoutesCollection { get; set; }
        public static ObservableCollection<string> DirectionCollection { get; set; }
        public static ObservableCollection<Stop> StopCollection { get; set; }
        public static HttpResponseMessage saved { get; set; }
        public static List<string> outboundStops = new List<string>();
        public static List<string> inboundStops = new List<string>();
        public static List<Stop> StopsList = new List<Stop>();
        public static List<Routes> RoutesList;
        public static List<Stop> FoundStops = new List<Stop>();
        public static string title, stopID, lat, lon, tag;
        private static string selectedRoute;

        public static void LoadStops()
        {
            RoutesList = DatabaseHelper.QueryForRoutes();
            RoutesCollection = new ObservableCollection<string>();
            DirectionCollection = new ObservableCollection<string>();
            StopCollection = new ObservableCollection<Stop>();

            foreach (Routes s in RoutesList)
            {
                RoutesCollection.Add(s.Title);
            }

            IsDataLoaded = true;
        }

        public static void RouteSelected(object sender, SelectionChangedEventArgs e)
        {
            string selectedRoute = ((ComboBox)sender).SelectedItem.ToString();
            //MainPage.dirComboBox
            if (DirectionCollection.Count != 0) DirectionCollection.Clear();
            LoadDirections(selectedRoute);
        }

        private static async void LoadDirections(string _route)
        {
            string URL = "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r=";
            int i = _route.IndexOf('-');
            _route = _route.Substring(0, i);
            selectedRoute = _route;
            URL = URL + _route;

            var response = new HttpResponseMessage();
            var client = new HttpClient();
            XDocument xmlDoc = new XDocument();
            string reader;

            if (saved != null) response = saved;

            //Make sure to ppull from network not cache everytime predictions are refreshed 
            response.Headers.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            client.DefaultRequestHeaders.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            if (response.Content != null) client.DefaultRequestHeaders.IfModifiedSince = response.Content.Headers.Expires;
            response = await client.GetAsync(new Uri(URL));
            response.Content.Headers.Expires = System.DateTime.Now;

            saved = response;

            reader = await response.Content.ReadAsStringAsync();
            xmlDoc = XDocument.Parse(reader);

            GetDirections(xmlDoc);            
        }

        private static void GetDirections(XDocument doc)
        {
            IEnumerable<XElement> tagElements;
            IEnumerable<XElement> rootElement =
                from e in doc.Descendants("route")
                select e;
            IEnumerable<XElement> elements = 
                from d in rootElement.ElementAt(0).Elements("stop")
                select d;

            //Add all route's stops to a collection
            foreach (XElement el in elements)
            {
                title = el.Attribute("title").Value;
                stopID = el.Attribute("stopId").Value;
                lon = el.Attribute("lon").Value;
                lat = el.Attribute("lat").Value;
                tag = el.Attribute("tag").Value;

                StopsList.Add(new Stop(title, stopID, tag, lon, lat));
            }

            //Move to direction element
            elements =
                from d in rootElement.ElementAt(0).Elements("direction")
                select d;

            foreach (XElement el in elements)
            {   
                //Add direction title
                DirectionCollection.Add(el.Attribute("title").Value);

                if(el.Attribute("name").Value == "Inbound")
                {
                    //Get all stop elements under direction element
                    tagElements =
                        from x in el.Elements("stop")
                        select x;

                    if (inboundStops.Count != 0) inboundStops.Clear();
                    //Add tags for direction to a collection
                    foreach (XElement y in tagElements)
                    {
                        inboundStops.Add(y.Attribute("tag").Value);
                    }
                } else if (el.Attribute("name").Value == "Outbound")
                {
                    //Get all stop elements under direction element
                    tagElements =
                        from x in el.Elements("stop")
                        select x;

                    if (outboundStops.Count != 0) outboundStops.Clear();
                    //Add tags for direction to a collection
                    foreach (XElement y in tagElements)
                    {
                        outboundStops.Add(y.Attribute("tag").Value);
                    }
                }
            }
        }

        public static void DirSelected(object sender, SelectionChangedEventArgs e)
        {
            string selectedDir = ((ComboBox)sender).SelectedItem.ToString();
            if (StopCollection.Count != 0) StopCollection.Clear();
            LoadStops(selectedDir);
        }

        private static void LoadStops(string _dir)
        {
            if(StopCollection.Count != 0) StopCollection.Clear();

            if(_dir.Contains("Inbound"))
            {
                foreach(string s in inboundStops)
                {
                    FoundStops = StopsList.FindAll(z => z.tag == s);
                    StopCollection.Add(new Stop(FoundStops[0].title, FoundStops[0].stopID, FoundStops[0].tag, FoundStops[0].lon.ToString(), FoundStops[0].lat.ToString()));
                }
            }
            else if(_dir.Contains("Outbound"))
            {
                foreach(string s in outboundStops)
                {
                    FoundStops = StopsList.FindAll(z => z.tag == s);
                    StopCollection.Add(new Stop(FoundStops[0].title, FoundStops[0].stopID, FoundStops[0].tag, FoundStops[0].lon.ToString(), FoundStops[0].lat.ToString()));
                }
            }
        }

        public static async void StopSelected(object sender, SelectionChangedEventArgs e)
        {
            Stop selectedStop = ((ComboBox)sender).SelectedItem as Stop;
            string url = "http://webservices.nextbus.com/service/publicXMLFeed?command=predictions&a=sf-muni&stopId=" + selectedStop.stopID + "&routeTag=" + selectedRoute;

            await MainPage.searchMap.TrySetViewAsync(new Geopoint(new BasicGeoposition() { Latitude = selectedStop.lat, Longitude = selectedStop.lon }), 16.0);
            PredictionModel.GetSearchPredictions(selectedStop, selectedRoute, url);
        }
    }

    public class Stop
    {
        public string title { get; set; }
        public string stopID;
        public double lon;
        public double lat;
        public string tag;

        public Stop() {}

        public Stop(string _title, string _id, string _tag, string _lon, string _lat)
        {
            title = _title;
            stopID = _id;
            lat = double.Parse(_lat);
            lon = double.Parse(_lon);
            tag = _tag;
        }
    }
}
