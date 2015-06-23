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

        private ApplicationDataContainer _localSettings;
        private Task _initialize;

        public MainViewModel()
        {
            _initialize = LoadAsync();
            DatabaseHelper.FavoritesChanged += LoadFavorites;
        }

        private async Task LoadAsync()
        {
            NearbyStops = new ObservableCollection<Stop>();
            FavoriteStops = new ObservableCollection<Stop>();

            LoadFavorites();
            await UpdateNearbyStops();
        }

        public async Task UpdateNearbyStops()
        {
            //Make sure user has given permission to access location
            await LocationHelper.UpdateLocation();

            if (LocationHelper.Location != null)
            {
                NoStopsText = "";
                List<Stop> stops = await DatabaseHelper.QueryForNearby(0.5);

                //Get distance to each stop
                foreach (Stop stop in stops)
                {
                    //stop.Distance = LocationHelper.GetDistance(stop.Latitude, stop.Longitude);
                    stop.DistanceAsDouble = LocationHelper.GetDistance(stop.Latitude, stop.Longitude);
                }

                //Sort list of stops by distance
                IEnumerable<Stop> sortedList =
                    from s in stops
                    orderby s.DistanceAsDouble
                    select s;

                //Add stops to listview with max of 15
                if (NearbyStops.Any())
                {
                    for(int i=0; i<NearbyStops.Count; i++)
                    {
                        NearbyStops.RemoveAt(i);
                        NearbyStops.Insert(i, sortedList.ElementAt(i));
                        //if (NearbyStops.Count >= 15) break;
                        //NearbyStops.Add(new Stop(stop.StopName, stop.Routes, stop.StopTags, stop.Latitude, stop.Longitude, stop.Distance))
                    }
                }
                else
                {
                    foreach (Stop stop in sortedList)
                    {
                        //NearbyStops.Add(new Stop(stop.StopName, stop.Routes, stop.StopTags, stop.Latitude, stop.Longitude, stop.Distance));
                        NearbyStops.Add(stop);
                        if (NearbyStops.Count >= 15) break;
                    }
                }

                SyncFavoriteIds();
            }
            else
            {
                NoStopsText = "No nearby stops found";
            }
        }

        public void FavoritesDistances()
        {
            LocationHelper.FavoritesDistances(FavoriteStops);
        }

        public void SortFavorites()
        {
            var sortedFavorites = new List<Stop>(FavoriteStops.OrderBy(s => s.DistanceAsDouble));
            int i= 0;
            //MainViewModel.FavoritesStops.Clear();
            foreach (Stop stop in sortedFavorites)
            {
                FavoriteStops.RemoveAt(i);
                FavoriteStops.Insert(i,stop);
                i++;
            }
        }

        private void LoadFavorites()
        {
            List<FavoriteData> favorites = DatabaseHelper.FavoritesList;

            if (favorites.Count == 0)
            {
                NoFavoritesText = "Add favorites by pressing &#xE00A; on a stop";
            }
            else
            {
                NoFavoritesText = "";
                FavoriteStops.Clear();
                foreach (FavoriteData fav in favorites)
                {
                    Stop favStop = new Stop(fav.Name, "", fav.Routes, fav.Tags, fav.Lat, fav.Lon);
                    favStop.favId = fav.Id;
                    FavoriteStops.Add(favStop);
                }
            }
            
            //Check if any stops in NearbyStops are also favorites so users have the ability to remove them
            SyncFavoriteIds();          
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

    