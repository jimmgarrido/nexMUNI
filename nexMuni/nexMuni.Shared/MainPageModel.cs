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

namespace nexMuni
{
    class MainPageModel
    {
        public static ObservableCollection<StopData> nearbyStops = new ObservableCollection<StopData>();
        public static ObservableCollection<StopData> favoritesStops = new ObservableCollection<StopData>();

        //public static bool nearbyEmpty { get; set; }
        //public  bool favEmpty { get; set; }
        public static bool IsDataLoaded { get; set; }

        public static void LoadData()
        {
            //nearbyEmpty = false;

            DatabaseHelper.LoadFavorites();
            LocationHelper.UpdateNearbyList();

            //MainPage.nearbyText.Text = "DONE";
            IsDataLoaded = true; 
        }
    }
}

    