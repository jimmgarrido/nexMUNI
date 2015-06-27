using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using SQLite;
using Windows.Devices.Geolocation;

namespace nexMuni.DataModels
{
    public class Route
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }

        //public List<RouteDirection> Directions { get; set; } 

        private string predictions1 = String.Empty;
        private string predictions2 = String.Empty;

        public Geopoint stopLocation;

        public string RouteName { get; private set; }
        public string RouteNumber { get; private set; }
        public List<RouteDirection> Directions { get; set; }
        public string Dir1 { get; set; }
        public string Dir2 { get; set; }

        public string Times1 { get; set; }
        public string Times2 { get; set; }

        public Route() { }

        public Route(string name, string num)
        {
            RouteName = name;
            RouteNumber = num;
            Title = string.Format("{0}-{1}", num, name);
            Directions = new List<RouteDirection>();
        }

        public void UpdateTimes(List<RouteDirection> directions)
        {
            for(int i=0; i<directions.Count; i++)
            {
                Directions[i].SetTimes(directions[i].Times);
            }
        }

        public void AddLocationInfo(double lat, double lon)
        {
            stopLocation = new Geopoint(new BasicGeoposition { Latitude = lat, Longitude = lon });
        }
    }
}
