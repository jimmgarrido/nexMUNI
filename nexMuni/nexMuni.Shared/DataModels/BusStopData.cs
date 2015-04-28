using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni.DataModels
{
    [Table("BusStops")]
    public class BusStopData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string StopName { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Routes { get; set; }
        public string StopTags { get; set; }
        public double Distance { get; set; }
    }
}
