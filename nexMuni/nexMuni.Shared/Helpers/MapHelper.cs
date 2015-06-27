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
using nexMuni.DataModels;

namespace nexMuni.Helpers
{
    class MapHelper
    {
        public static async Task<List<MapPolyline>> ParseRoutePath(XDocument document)
        {
            IEnumerable<XElement> elements =
                from e in document.Element("body").Element("route").Elements("path")
                select e;

            var path = new List<MapPolyline>();
            var points = new List<BasicGeoposition>();

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

            return path;
        }

        public static async Task<List<Bus>> ParseBusLocations(XDocument document)
        {
            var vehicles = new List<Bus>();

            var elements =
                from e in document.Element("body").Elements("vehicle")
                select e;

            foreach(XElement bus in elements)
            {
                if (bus.Attribute("dirTag") != null)
                {
                    vehicles.Add(new Bus(bus.Attribute("id").Value, bus.Attribute("heading").Value,
                        bus.Attribute("lat").Value, bus.Attribute("lon").Value, bus.Attribute("dirTag").Value));
                }
            }

            return vehicles;
        }
    }
}
