using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.Devices.Geolocation;
using Windows.Storage;
using System.Collections.ObjectModel;

namespace nexMuni
{
    class LocationHelper
    {
        public static async void UpdateNearbyList()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            var position = await geolocator.GetGeopositionAsync();

            FindNearby(position.Coordinate.Point);
        }

        private static void FindNearby(Geopoint location)
        {
            //code to create bounds
            //query db with bounds
            IEnumerable<BusStop> results = DatabaseHelper.QueryDatabase();
            //StopResults = new ObservableCollection<StopData>();
            int counter = 0;
            NearbyModel.nearbyStops.Clear();
            foreach (BusStop d in results)
            {
                NearbyModel.nearbyStops.Add(new StopData(d.RouteTitle, d.Routes));
                counter++;
                if (counter > 10) break;
            }

            //string contents;
            //int counter = 0;

            //StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///db/sorted.txt"));
            //using (StreamReader sRead = new StreamReader(await file.OpenStreamForReadAsync()))

            //    do
            //    {
            //        contents = await sRead.ReadLineAsync();

            //        string[] split = contents.Split('%');
            //        NearbyModel.nearbyStops.Add(new StopData(split[0], split[3]));
            //        counter++;
            //    } while (!sRead.EndOfStream && (counter < 10));
        }
    }
}
