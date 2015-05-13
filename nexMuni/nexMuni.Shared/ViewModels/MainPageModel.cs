using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using nexMuni.DataModels;
using nexMuni.Helpers;
using nexMuni.Views;

namespace nexMuni.ViewModels
{
    public class MainPageModel
    {
        public static ObservableCollection<StopData> NearbyStops { get; private set; }
        public static ObservableCollection<StopData> FavoritesStops { get; private set;}
        public static bool IsDataLoaded { get; private set; }

        private static ApplicationDataContainer localSettings;

        public static async void LoadData()
        {
            NearbyStops = new ObservableCollection<StopData>();
            FavoritesStops = new ObservableCollection<StopData>();

            localSettings = ApplicationData.Current.LocalSettings;

            await DatabaseHelper.CheckDatabases();
            await LoadFavorites();
            await UpdateNearbyStops();

            IsDataLoaded = true;
        }

        public static async Task UpdateNearbyStops()
        {
            //Make sure user has giiven permission to access location
            //if(localSettings.Values.ContainsKey("locationAccess"))
            //{
            //    if(localSettings.Values["locationAccess"] == "yes")
            //    {
                    NearbyStops.Clear();
                    await LocationHelper.UpdateLocation();

                    if (LocationHelper.phoneLocation != null)
                    {
                        MainPage.noNearbyText.Visibility = Visibility.Collapsed;
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
                            if (NearbyStops.Count >= 15) break;
                        }
                    }
                    else
                    {
                        await Task.Delay(400);
                        MainPage.mainPivot.SelectedIndex = 1;
                        MainPage.noNearbyText.Visibility = Visibility.Visible; 
                    }
            //    }
            //}
        }

        public static async Task LoadFavorites()
        {
            FavoritesStops.Clear();
            List<FavoriteData> favorites = await DatabaseHelper.GetFavorites();

            if (favorites.Count == 0)
            {
                MainPage.noFavsText.Visibility = Visibility.Visible;
            }
            else
            {
                MainPage.noFavsText.Visibility = Visibility.Collapsed;
                foreach (FavoriteData favorite in favorites)
                {
                    FavoritesStops.Add(new StopData(favorite.Name, favorite.Routes, favorite.Tags, 0.000, favorite.Lat, favorite.Lon, favorite.Id.ToString()));
                }
            }
            DatabaseHelper.SyncIDS();
        }
    }
}

    