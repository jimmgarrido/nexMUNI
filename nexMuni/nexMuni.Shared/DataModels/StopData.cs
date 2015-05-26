using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni.DataModels
{
    public class StopData
    {
        public string FavID { get; set; }
        public string Name { get; set; }
        public string Routes { get; set; }
        public string Distance { get; set; }
        public double DoubleDist { get; set; }
        public string Tags { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string StopID;

        public StopData() { }

        public StopData(string stopName, string routes, string _tags, double d, double _lat, double _lon)
        {
            Name = stopName;
            this.Tags = _tags;
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
            this.Tags = _tags;
            this.Routes = routes;
            Lat = _lat;
            Lon = _lon;
            FavID = _id;
            if (d != 0.000) Distance = d.ToString("F") + " mi";        
        }

        public StopData(string stopName, string _id, string _tag, string _lon, string _lat)
        {
            Name = stopName;
            StopID = _id;
            Lat = double.Parse(_lat);
            Lon = double.Parse(_lon);
            Tags = _tag;
        }
    }
}
