using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using nexMuni.DataModels;
using nexMuni.Helpers;
using nexMuni.Views;
using System.ComponentModel;

namespace nexMuni.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _noStopsText;
        private string _noFavoritesText;

        public ObservableCollection<Stop> NearbyStops { get; private set; }
        public ObservableCollection<Stop> FavoritesStops { get; private set;}
        public string NoStopsText
        {
            get
            {
                return _noStopsText;
            }
            set
            {
                _noStopsText = value;
                NotifyPropertyChanged("NoStopsText");
            }
        }
        public string NoFavoritesText
        {
            get
            {
                return _noFavoritesText;
            }
            set
            {
                _noFavoritesText = value;
                NotifyPropertyChanged("NoFavoritesText");
            }
        }

        private ApplicationDataContainer _localSettings;
        private Task _initialize;

        public MainViewModel()
        {
            _initialize = LoadAsync();
        }

        private async Task LoadAsync()
        {
            NearbyStops = new ObservableCollection<Stop>();
            FavoritesStops = new ObservableCollection<Stop>();

            await LoadFavoritesAsync();
            await UpdateNearbyStops();
        }

        public async Task UpdateNearbyStops()
        {
            //Make sure user has given permission to access location
            NearbyStops.Clear();
            await LocationHelper.UpdateLocation();

            if (LocationHelper.Location != null)
            {
                NoStopsText = "";
                List<BusStopData> stops = await DatabaseHelper.QueryForNearby(0.5);

                //Get distance to each stop
                foreach (BusStopData stop in stops)
                {
                    stop.Distance = LocationHelper.GetDistance(stop.Latitude, stop.Longitude);
                }

                //Sort list of stops by distance
                IEnumerable<BusStopData> sortedList =
                    from s in stops
                    orderby s.Distance
                    select s;

                //Add stops to listview with max of 15
                foreach (BusStopData stop in sortedList)
                {
                    NearbyStops.Add(new Stop(stop.StopName, stop.Routes, stop.StopTags, stop.Latitude, stop.Longitude, stop.Distance));
                    if (NearbyStops.Count >= 15) break;
                }
            }
            else
            {
                NoStopsText = "No nearby stops found";
            }
            //    }
            //}
        }

        private async Task LoadFavoritesAsync()
        {
            List<FavoriteData> favorites = await DatabaseHelper.GetFavoritesAsync();

            if (favorites.Count == 0)
            {
                NoFavoritesText = "Add favorites by pressing &#xE00A; on a stop";
            }
            else
            {
                NoFavoritesText = "";
                FavoritesStops.Clear();
                foreach (FavoriteData fav in favorites)
                {
                    Stop favStop = new Stop(fav.Name, "", fav.Routes, fav.Tags, fav.Lat, fav.Lon);
                    favStop.favId = fav.Id;
                    FavoritesStops.Add(favStop);
                }
            }
            
            //Check if any stops in NearbyStops are also favorites so users have the ability to remove them
            SyncFavoriteIds();
            
        }
        private void SyncFavoriteIds()
        {
            foreach (Stop fav in FavoritesStops)
            {
                if(NearbyStops.Any(s => s.StopName == fav.StopName))
                {

                }
            }
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

    