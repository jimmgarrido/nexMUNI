using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.Devices.Geolocation;
using System.Collections.ObjectModel;

namespace nexMuni.DataModels
{
    public class Route : INotifyPropertyChanged, IDisposable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string RouteName { get; private set; }
        public string RouteNumber { get; private set; }
        //public List<RouteDirection> Directions { get
        //    {
        //        return _directions;
        //    }
        //    set
        //    {
        //        _directions = value;
        //        NotifyPropertyChanged("Directions");
        //    }
        //}
        public ObservableCollection<RouteDirection> Directions { get; set; }

        private string predictions1 = String.Empty;
        private string predictions2 = String.Empty;
        public Geopoint stopLocation;


        public Route() { }

        public Route(string name, string num)
        {
            RouteName = name;
            RouteNumber = num;
            Title = string.Format("{0}-{1}", num, name);
            //Directions = new List<RouteDirection>();
            Directions = new ObservableCollection<RouteDirection>();
        }

        public void UpdateTimes(List<RouteDirection> updatedDirections)
        {
            if(updatedDirections.Count > Directions.Count)
            {
                Directions.Clear();

                for (int i = 0; i < updatedDirections.Count; i++)
                {
                    Directions.Add(updatedDirections[i]);
                    Directions[i].SetTimes(updatedDirections[i].Times);
                }
            }
            else
            {
                for (int i = 0; i < updatedDirections.Count; i++)
                {
                    Directions[i].SetTimes(updatedDirections[i].Times);
                }
            }
        }

        public void AddLocationInfo(double lat, double lon)
        {
            stopLocation = new Geopoint(new BasicGeoposition { Latitude = lat, Longitude = lon });
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
        // ~Route() {
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
}
