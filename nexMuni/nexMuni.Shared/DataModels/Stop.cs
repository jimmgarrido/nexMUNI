using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
#if WINDOWS_PHONE_APP
using SQLite;
#elif WINDOWS_UWP
using SQLite.Net.Attributes;
#endif

namespace nexMuni.DataModels
{
    [Table("BusStops")]
    public class Stop : INotifyPropertyChanged
    {
        private string _stringDistance;
        private double _doubleDistance;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string StopName { get; set; }
        public string Routes { get; set; }
        public string StopTags { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string DistanceAsString
        {
            get
            {
                return _stringDistance;
            }
            set
            {
                _stringDistance = value;
                NotifyPropertyChanged("DistanceAsString");
            }
        }
        public double DistanceAsDouble
        {
            get
            {
                return _doubleDistance;
            }
            set
            {
                _doubleDistance = value;
                DistanceAsString = DistanceAsDouble.ToString("F") + "mi";
            }
        }

        public string stopId;
        public int favId;

        public Stop() {}

        public Stop(string name, string id, string routes, string tags, double lat, double lon )
        {
            StopName = name;
            Routes = routes;
            StopTags = tags;
            Latitude = lat;
            Longitude = lon;
            stopId = id;
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

        #region INotify Methods
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
};
