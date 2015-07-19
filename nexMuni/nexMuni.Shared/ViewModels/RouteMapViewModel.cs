using nexMuni.DataModels;
using nexMuni.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Maps;

namespace nexMuni.ViewModels
{
    public class RouteMapViewModel : INotifyPropertyChanged
    {
        private Route _selectedRoute;

        public Route SelectedRoute
        {
            get
            {
                return _selectedRoute;
            }
            set
            {
                _selectedRoute = value;
                NotifyPropertyChanged("SelectedRoute");
            }
        }

        public RouteMapViewModel(Route route)
        {
            SelectedRoute = route;
        }

        public async Task<List<MapPolyline>> GetRoutePath()
        {
            var xmlDoc = await WebHelper.GetRoutePathAsync(SelectedRoute.RouteNumber);
            return await MapHelper.ParseRoutePath(xmlDoc);
        }

        public async Task<List<Bus>> GetBusLocations()
        {
            var xmlDoc = await WebHelper.GetBusLocationsAsync(SelectedRoute.RouteNumber);
            return MapHelper.ParseBusLocations(xmlDoc);
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
}
