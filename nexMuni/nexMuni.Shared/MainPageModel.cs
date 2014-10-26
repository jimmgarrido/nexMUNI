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

namespace nexMuni
{
    class MainPageModel
    {
        public static ObservableCollection<StopData> nearbyStops = new ObservableCollection<StopData>();
        public static ObservableCollection<StopData> favoritesStops = new ObservableCollection<StopData>();
        public static bool IsDataLoaded { get; set; }

        public static async void LoadData()
        {
            await DatabaseHelper.CheckDB();
            LocationHelper.UpdateNearbyList();
            DatabaseHelper.CheckFavDB();

            await SearchModel.LoadData();

            IsDataLoaded = true;
        }

        public static void DisplayResults(List<BusStop> r)
        {
            int counter = 0;

            //Get ditance to each stop
            foreach (BusStop stop in r)
            {
                stop.Distance = LocationHelper.Distance(stop.Latitude, stop.Longitude);
            }

            //Sort list of stops by distance
            IEnumerable<BusStop> sortedList =
                from s in r
                orderby s.Distance
                select s;

            //Add stops to listview with max of 15
            foreach (BusStop d in sortedList)
            {
                if (counter < 15)
                {
                    MainPageModel.nearbyStops.Add(new StopData(d.StopName, d.Routes, d.StopTags, d.Distance, d.Latitude, d.Longitude));
                    counter++;
                }
                else break;
            }
        }

        public static void DisplayResults(List<FavoriteData> r)
        {
            MainPage.noFavsText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            foreach (FavoriteData s in r)
            {
                MainPageModel.favoritesStops.Add(new StopData(s.Name, s.Routes, s.Tags, 0.000, s.Lat, s.Lon, s.Id.ToString()));
            }

            DatabaseHelper.SyncIDS();
        }
    }
}

    