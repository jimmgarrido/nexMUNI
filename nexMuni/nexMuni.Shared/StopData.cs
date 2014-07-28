using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni
{
    public class StopData
    {
        public string FavID { get; set; }
        public string Name { get; set; }
        public string [] RoutesSplit { get; set; }
        public string Routes { get; set; }
        public string [] TagsSplit { get; set; }
        public string Distance { get; set; }
        public double TrueDistance { get; set; }
        public string Tags { get; set; }
        public bool Favorite { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public StopData() { }

        public StopData(string stopName, string routes)
        {
            Name = stopName;
            this.RoutesSplit = routes.Split(',');
            this.Routes = routes;
        }

        public StopData(string stopName, string routes, string _tags, double d)
        {
            Name = stopName;
            this.RoutesSplit = routes.Split(',');
            RoutesSplit[0] = " " + RoutesSplit[0];
            this.Tags = _tags;
            this.TagsSplit = _tags.Split(',');
            this.Routes = routes;
            Distance = d.ToString("F") + " mi";
            TrueDistance = d;
        }

        public StopData(string stopName, string routes, string _tags, int id)
        {
            Name = stopName;
            this.Tags = _tags;
            this.Routes = routes;
            FavID = id.ToString();
        }

        public StopData(string stopName, string routes, double _d, string _tags, string id)
        {
            Name = stopName;
            this.Tags = _tags;
            this.Routes = routes;
            FavID = id.ToString();
            Distance = _d.ToString("F") + " mi";
            TrueDistance = _d;
        }

        public void AddCoordinates(double _lat, double _lon)
        {
            Lat = _lat;
            Lon = _lon;
        }
    }
}
