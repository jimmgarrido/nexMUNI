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
        public double DoubleDist { get; set; }
        public string Tags { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public StopData() { }

        public StopData(string stopName, string routes, string _tags, double d, double _lat, double _lon)
        {
            Name = stopName;
            this.RoutesSplit = routes.Split(',');
            RoutesSplit[0] = " " + RoutesSplit[0];
            this.Tags = _tags;
            this.TagsSplit = _tags.Split(',');
            this.Routes = routes;
            Distance = d.ToString("F") + " mi";
            DoubleDist = d;
            Lat = _lat;
            Lon = _lon;
            FavID = "";
        }

        public StopData(string stopName, string routes, string _tags, double d, double _lat, double _lon, string _id)
        {
            Name = stopName;
            this.RoutesSplit = routes.Split(',');
            RoutesSplit[0] = " " + RoutesSplit[0];
            this.Tags = _tags;
            this.TagsSplit = _tags.Split(',');
            this.Routes = routes;
            Lat = _lat;
            Lon = _lon;
            FavID = _id;
            if (d != 0.000) Distance = d.ToString("F") + " mi";        
        }
    }
}
