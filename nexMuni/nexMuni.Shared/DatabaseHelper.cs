using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using Windows.Devices.Geolocation;

namespace nexMuni
{
    class DatabaseHelper
    {
        public static IList<BusStop> QueryDatabase(double[][] b, Geopoint l, double d)
        {
            var db = new SQLiteConnection("db/muni.sqlite");

            string query = "SELECT * FROM BusStops WHERE Longitude BETWEEN " + b[3][1] + " AND " + b[1][1] + " AND Latitude BETWEEN " + b[2][0] + " AND " + b[0][0];
            IList<BusStop> r = db.Query<BusStop>(query);
            
            if(r.Count == 0  || r.Count < 10)
            {
                LocationHelper.FindNearby(l, d += 0.50);
            } else db.Close();
            
            return r;
        }
    }

    public class BusStop
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string RouteTitle { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Routes { get; set; }
        public string StopIDs { get; set; }
        public double Distance { get; set; }
    }


}
