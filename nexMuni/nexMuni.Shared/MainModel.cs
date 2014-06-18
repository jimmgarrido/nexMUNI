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
    class MainModel
    {
        public static ObservableCollection<StopData> nearbyStops;
        public static bool IsDataLoaded { get; set; }

        public static void LoadData()
        {
            nearbyStops = new ObservableCollection<StopData>();
            IsDataLoaded = true;
            nearbyStops.Add(new StopData("Junipero Serra Blvd & Ocean Ave", "28, 29, M, 17, 28L"));
            UpdateLocation();
        }

        public static async void UpdateLocation()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            var position = await geolocator.GetGeopositionAsync();

            //await locationMap.TrySetViewAsync(position.Coordinate.Point, 17, 0, 0);

            GetNearby(position.Coordinate.Point);
        }

        public static async void GetNearby(Geopoint location)
        {
            string contents;
            int counter = 0;

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///db/sorted.txt"));
            using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))

                do
                {
                    contents = await sRead.ReadLineAsync();

                    string[] split = contents.Split('%');
                    nearbyStops.Add(new StopData(split[0], split[3]));
                    counter++;
                } while (!sRead.EndOfStream && (counter < 10));
        }
    }
}

    