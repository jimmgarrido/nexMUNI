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

namespace nexMuni.Helpers
{
    class MapHelper
    {
        public static async void LoadDoc(string route)
        {
            string URL = "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r=";
            URL = URL + route;

            var response = new HttpResponseMessage();
            var client = new HttpClient();

            //Make sure to pull from network not cache everytime predictions are refreshed 
            response.Headers.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            client.DefaultRequestHeaders.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            if (response.Content != null) client.DefaultRequestHeaders.IfModifiedSince = response.Content.Headers.Expires;
            response = await client.GetAsync(new Uri(URL));
            response.Content.Headers.Expires = DateTime.Now;

            var reader = await response.Content.ReadAsStringAsync();
            GetPoints(XDocument.Parse(reader));
        }

        public static void GetPoints(XDocument doc)
        {
            List<BasicGeoposition> positions = new List<BasicGeoposition>();
            List<MapPolyline> route = new List<MapPolyline>();

            IEnumerable<XElement> rootElement =
                from e in doc.Descendants("route")
                select e;
            IEnumerable<XElement> elements =
                from d in rootElement.ElementAt(0).Elements("path")
                select d;
            int x = 0;
            
            foreach (XElement el in elements)
            {
                var subElements = from p in el.Elements("point")
                    select p;

                if (positions.Count > 0) positions.Clear();
                positions.AddRange(subElements.Select(e => new BasicGeoposition() {Latitude = double.Parse(e.Attribute("lat").Value), Longitude = double.Parse(e.Attribute("lon").Value)}));
                route.Add(new MapPolyline());
                route[x].StrokeColor = Color.FromArgb(255, 179, 27, 27);
                route[x].StrokeThickness = 2.00;
                route[x].ZIndex = 99;
                route[x].Path = new Geopath(positions);
                route[x].Visible = true;
                RouteMap.routeMap.MapElements.Add(route[x]);
                x++;
            }

            if (LocationHelper.phoneLocation != null)
            {
                Image icon = new Image
                {
                    Source = new BitmapImage(new Uri("ms-appx:///Assets/Location.png")),
                    Width = 25,
                    Height = 25
                };

                RouteMap.routeMap.Children.Add(icon);
                MapControl.SetNormalizedAnchorPoint(icon, new Point(0.5, 0.5));
                MapControl.SetLocation(icon, LocationHelper.phoneLocation.Coordinate.Point);
            }
        }
    }
}
