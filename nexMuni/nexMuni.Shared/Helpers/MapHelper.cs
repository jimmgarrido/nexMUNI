using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using nexMuni.Views;
using System.Threading.Tasks;

namespace nexMuni.Helpers
{
    class MapHelper
    {
        private static HttpClient client = new HttpClient();
        private static List<MapPolyline> path = new List<MapPolyline>();
        private static List<BasicGeoposition> points = new List<BasicGeoposition>();

        public static async Task<List<MapPolyline>> LoadDoc(string route)
        {
            path.Clear(); 

            if (route.Equals("Powell/Mason Cable Car")) route = "59";
            else if (route.Equals("Powell/Hyde Cable Car")) route = "60";
            else if (route.Equals("California Cable Car")) route = "61";
            else if(route.Contains('-'))
            {
                route = route.Substring(0, route.IndexOf('-'));
            }

            string url = "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r=";
            url = url + route;
           
            try
            {
                HttpResponseMessage response = await client.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();
                string reader = await response.Content.ReadAsStringAsync();
                GetPath(XDocument.Parse(reader));
            }
            catch (Exception)
            {
                ErrorHandler.NetworkError("Error getting route info. Check network connection and try again.");
            }
            
            return path;
        }

        private static void GetPath(XDocument doc)
        {
            IEnumerable<XElement> elements =
                from e in doc.Element("body").Element("route").Elements("path")
                select e;
            
            foreach (XElement el in elements)
            {
                points.Clear();

                var subElements = from p in el.Elements("point")
                    select p;

                points.AddRange(subElements.Select(e => new BasicGeoposition() {Latitude = double.Parse(e.Attribute("lat").Value), Longitude = double.Parse(e.Attribute("lon").Value)}));
                path.Add(new MapPolyline
                {
                    Path = new Geopath(points),
                    StrokeColor = Color.FromArgb(255, 179, 27, 27),
                    StrokeThickness = 2.00,
                    ZIndex = 99
                });
            }
        }
    }
}
