using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace nexMuni.DataModels
{
    [Table("BusStops")]
    public class Stop
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string StopName { get; set; }
        public string Routes { get; set; }
        public string StopTags { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string DistanceAsString
        {
            get { return DistanceAsDouble.ToString("F") + "mi"; }
        }
        public double DistanceAsDouble { get; private set; }

        private int _favId;

        public Stop(string name, string routes, string tags, double lat, double lon )
        {
            StopName = name;
            Routes = routes;
            StopTags = tags;
            Latitude = lat;
            Longitude = lon;
        }

        public Stop(string name, string routes, string tags, double lat, double lon, double distance)
        {
            StopName = name;
            Routes = routes;
            StopTags = tags;
            Latitude = lat;
            Longitude = lon;
            DistanceAsDouble = distance;
        }

        public void SetFavoriteId(int id)
        {
            _favId = id;
        }
    }
};
