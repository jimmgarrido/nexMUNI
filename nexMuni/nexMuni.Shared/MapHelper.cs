using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Services.Maps;
using Windows.UI;

namespace nexMuni
{
    class MapHelper
    {
        public static async void LoadDoc(string route)
        {
            string URL = "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r=";
            URL = URL + route;
            HttpResponseMessage saved = null;

            var response = new HttpResponseMessage();
            var client = new HttpClient();
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
            GetPoints(XDocument.Parse(reader));
        }

        public async static void GetPoints(XDocument doc)
        {
            MapService.ServiceToken = "jjA5Pn6AN4an5lgqKNN_Rg";
            List<Geopoint> positions = new List<Geopoint>();
            IEnumerable<Geopoint> points;
            IEnumerable<XElement> subElements;
            MapRoute route;
            MapRouteView view;

            IEnumerable<XElement> rootElement =
                from e in doc.Descendants("route")
                select e;
            IEnumerable<XElement> elements =
                from d in rootElement.ElementAt(0).Elements("path")
                select d;

            foreach(XElement el in elements)
            {
                subElements =
                    from p in el.Elements("point")
                    select p;

                foreach(XElement e in subElements)
                {
                    positions.Add(new Geopoint(new BasicGeoposition() { Latitude = Double.Parse(e.Attribute("lat").Value), Longitude = Double.Parse(e.Attribute("lon").Value) }));
                }
            }
            points = positions.AsEnumerable<Geopoint>();
            MapRouteFinderResult result = await MapRouteFinder.GetDrivingRouteFromWaypointsAsync(points);
            if(result.Status == MapRouteFinderStatus.Success)
            {
                route = result.Route;
                view = new MapRouteView(route);
                view.RouteColor = Colors.Black;
                RouteMap.routeMap.Routes.Add(view);
                RouteMap.routeMap.TrySetViewBoundsAsync(route.BoundingBox, null, MapAnimationKind.None);
            }
        }
    }
}
