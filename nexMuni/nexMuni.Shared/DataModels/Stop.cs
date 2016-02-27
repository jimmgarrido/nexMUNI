using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SQLite;

namespace nexMuni.DataModels
{
    [Table("BusStops")]
    public class Stop : INotifyPropertyChanged, IDisposable
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

        public Stop(string name, string tag, double lat, double lon)
        {

        }

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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    PropertyChanged = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Stop() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        #endregion
    }
};
