using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Data;
using System.Xml;
using Windows.Data.Xml;
using Windows.Storage;
using System.IO;
using System.Threading.Tasks;
using nexMuni.DataModels;

namespace nexMuni
{
    public class MainPageModel
    {
        public static ObservableCollection<StopData> NearbyStops { get; private set; }
        public static ObservableCollection<StopData> FavoritesStops { get; private set;}
        public static bool IsDataLoaded { get; private set; }

        public MainPageModel() 
        {
            NearbyStops = new ObservableCollection<StopData>();
            FavoritesStops = new ObservableCollection<StopData>();
        }

        public static async void LoadData()
        {
            NearbyStops = new ObservableCollection<StopData>();
            FavoritesStops = new ObservableCollection<StopData>();

            await DatabaseHelper.CheckDatabases();
            await LocationHelper.UpdateLocation();
            await UpdateNearbyStops();
            await LoadFavorites();

            //await SearchModel.LoadData();

            IsDataLoaded = true;
        }

        //public static void DisplayResults(List<FavoriteData> r)
        //{
        //    MainPage.noFavsText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            
        //}

        private static async Task UpdateNearbyStops()
        {
            NearbyStops.Clear();
            List<BusStopData> stops = await DatabaseHelper.QueryForNearby(0.5);

            //Get distance to each stop
            foreach (BusStopData stop in stops)
            {
                stop.Distance = LocationHelper.Distance(stop.Latitude, stop.Longitude);
            }

            //Sort list of stops by distance
            IEnumerable<BusStopData> sortedList =
                from s in stops
                orderby s.Distance
                select s;

            //Add stops to listview with max of 15
            foreach (BusStopData stop in sortedList)
            {
                NearbyStops.Add(new StopData(stop.StopName, stop.Routes, stop.StopTags, stop.Distance, stop.Latitude, stop.Longitude));
            }
        }

        private static async Task LoadFavorites()
        {
            FavoritesStops.Clear();
            List<FavoriteData> favorites = await DatabaseHelper.GetFavorites();

            foreach (FavoriteData favorite in favorites)
            {
                FavoritesStops.Add(new StopData(favorite.Name, favorite.Routes, favorite.Tags, 0.000, favorite.Lat, favorite.Lon, favorite.Id.ToString()));
            }

            DatabaseHelper.SyncIDS();
        }
    }
}

    