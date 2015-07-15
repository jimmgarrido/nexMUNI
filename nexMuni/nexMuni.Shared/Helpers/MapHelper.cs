using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml.Controls.Maps;
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

        public static List<Bus> ParseBusLocations(XDocument document)
        {
            var elements =
                from e in document.Element("body").Elements("vehicle")
                select e;

            return (from bus in elements where bus.Attribute("dirTag") != null 
                    select new Bus(bus.Attribute("id").Value, bus.Attribute("heading").Value,
                        bus.Attribute("lat").Value, bus.Attribute("lon").Value, bus.Attribute("dirTag").Value)).ToList();
        }
    }
}
