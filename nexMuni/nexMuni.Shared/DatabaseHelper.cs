using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace nexMuni
{
    class DatabaseHelper
    {
        public static IEnumerable<BusStop> QueryDatabase()
        {
            var db = new SQLiteConnection("db/muni.sqlite");

            return db.Query<BusStop>("select * from BusStops");
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
    }


}
