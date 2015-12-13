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
using Windows.UI.Xaml.Controls.Maps;
using Windows.Devices.Geolocation;

namespace nexMuni.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _noStopsText;
        private string _noFavoritesText;

        private int nearbyCount;
        private bool sorted = false;

        public ObservableCollection<Stop> NearbyStops { get; private set; }
        public ObservableCollection<Stop> FavoriteStops { get; private set;}
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

        public MainViewModel()
        {
            NearbyStops = new ObservableCollection<Stop>();
            FavoriteStops = new ObservableCollection<Stop>();

            DatabaseHelper.FavoritesChanged += LoadFavoritesAsync;
        }

        public async Task LoadAsync()
        {
            //LoadFavoritesAsync();
            nearbyCount = SettingsHelper.nearbyCount;
            await UpdateNearbyStopsAsync();
        }

        public async Task UpdateNearbyStopsAsync()
        {
            //Make sure user has given permission to access location
            await LocationHelper.UpdateLocation();

            if (LocationHelper.Location != null)
            {
                NoStopsText = "";
                List<Stop> stops = await Task.Run(() => DatabaseHelper.QueryForNearby(0.5));

                //Get distance to each stop
                foreach (Stop stop in stops)
                {
                    //stop.Distance = LocationHelper.GetDistance(stop.Latitude, stop.Longitude);
                    stop.DistanceAsDouble = await Task.Run(() => LocationHelper.GetDistance(stop.Latitude, stop.Longitude));
                }

                //Sort list of stops by distance
                IEnumerable<Stop> sortedList =
                    from s in stops
                    orderby s.DistanceAsDouble
                    select s;

                //Add stops to listview with a max of user selection
                if (NearbyStops.Any())
                {
                    while(NearbyStops.Any())
                    {
                        NearbyStops.RemoveAt(NearbyStops.Count - 1);
                    }

                    for (int j=0; j < nearbyCount; j++)
                    {
                        //NearbyStops.RemoveAt(j);
                        NearbyStops.Insert(j, sortedList.ElementAt(j));
                    }
                }
                else
                {
                    foreach (Stop stop in sortedList)
                    {
                        NearbyStops.Add(stop);
                        if (NearbyStops.Count >= nearbyCount) break;
                    }
                }

                SyncFavoriteIds();
            }
            else
            {
                NoStopsText = "No nearby stops found";
            }
        }

        public async Task<List<IEnumerable<BasicGeoposition>>> GetRoutePathAsync(string route)
        {
            var xmlDoc = await WebHelper.GetRoutePathAsync(route);
            return await Task.Run(() => MapHelper.ParseRoutePath(xmlDoc));
        }

        public void FavoritesDistances()
        {
            LocationHelper.FavoritesDistances(FavoriteStops);
        }

        public void SortFavorites()
        {
            var sortedFavorites = new List<Stop>(FavoriteStops.OrderBy(s => s.DistanceAsDouble));
            int i= 0;

            foreach (Stop stop in sortedFavorites)
            {
                FavoriteStops.RemoveAt(i);
                FavoriteStops.Insert(i,stop);
                i++;
            }
            sorted = true;
        }

        private async void LoadFavoritesAsync()
        {
            List<FavoriteData> favorites = DatabaseHelper.FavoritesList;
            FavoriteStops.Clear();

            if (favorites.Count == 0)
            {
                NoFavoritesText = "Add favorites by pressing \uE00A on a stop";
            }
            else
            {
                NoFavoritesText = "";
                foreach (FavoriteData fav in favorites)
                {
                    Stop favStop = new Stop(fav.Name, "", fav.Routes, fav.Tags, fav.Lat, fav.Lon);
                    favStop.favId = fav.Id;
                    FavoriteStops.Add(favStop);
                }
            }
            
            //Check if any stops in NearbyStops are also favorites so users have the ability to remove them
            SyncFavoriteIds();
            await Task.Run(() => FavoritesDistances());
            if (sorted) SortFavorites();
        }

        private void SyncFavoriteIds()
        {
            foreach (Stop stop in NearbyStops)
            {
                if(FavoriteStops.Any(fav => fav.StopName == stop.StopName))
                {
                    Stop tempStop = FavoriteStops.ToList().Find(s => s.StopName == stop.StopName);
                    stop.favId = tempStop.favId;
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

    