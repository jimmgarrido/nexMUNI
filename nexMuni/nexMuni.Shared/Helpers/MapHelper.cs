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
    public class MapHelper
    {
        public static List<IEnumerable<BasicGeoposition>> ParseRoutePath(XDocument document)
        {
            //IEnumerable<XElement> elements =
            //    from e in document.Element("body").Element("route").Elements("path")
            //    select e;

            //var path = new List<MapPolyline>();
            //var points = new List<BasicGeoposition>();

            //foreach (XElement el in elements)
            //{
            //    points.Clear();

            //    var subElements = from p in el.Elements("point")
            //        select p;

            //var points = from element in subElements
            //             select element => new BasicGeoposition() { Latitude = double.Parse(e.Attribute("lat").Value), Longitude = double.Parse(e.Attribute("lon").Value) };
            //points.AddRange(subElements.Select(e => new BasicGeoposition() { Latitude = double.Parse(e.Attribute("lat").Value), Longitude = double.Parse(e.Attribute("lon").Value) }));
            //    path.Add(new MapPolyline
            //    {
            //        Path = new Geopath(points),
            //        StrokeColor = Color.FromArgb(255, 179, 27, 27),
            //        StrokeThickness = 2.00,
            //        ZIndex = 99
            //    });
            //}

            //return path;

            var pathElements =
                from e in document.Element("body").Element("route").Elements("path")
                select e;

            //var path = new List<MapPolyline>();
            var pathPoints = new List<IEnumerable<BasicGeoposition>>();

            foreach (XElement element in pathElements)
            {
                var pointElements = from point in element.Elements("point")
                                  select point;

                var points = pointElements.Select(e => new BasicGeoposition() { Latitude = double.Parse(e.Attribute("lat").Value), Longitude = double.Parse(e.Attribute("lon").Value) });
                //path.Add(new MapPolyline
                //{
                //    Path = new Geopath(points),
                //    StrokeColor = Color.FromArgb(255, 179, 27, 27),
                //    StrokeThickness = 2.00,
                //    ZIndex = 99
                //});
                pathPoints.Add(points);
            }

            return pathPoints;
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
