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
#if WINDOWS_PHONE_APP
            var systemTray = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            systemTray.ProgressIndicator.Text = "Getting Location";
            systemTray.ProgressIndicator.ProgressValue = null;
#endif
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            var position = await geolocator.GetGeopositionAsync();

#if WINDOWS_PHONE_APP
            systemTray.ProgressIndicator.ProgressValue = null;
            systemTray.ProgressIndicator.Text = "Locating Stops";
#endif

            FindNearby(position.Coordinate.Point, 0.5, 0);

#if WINDOWS_PHONE_APP           
            systemTray.ProgressIndicator.ProgressValue = 0;
            systemTray.ProgressIndicator.Text = "nexMuni";
#endif
        }

        public static void FindNearby(Geopoint location, double dist, int count)
        {
            if (count < 5)
            {
                double latitude = location.Position.Latitude;
                double longitude = location.Position.Longitude;
                //code to create bounds
                double[][] bounds = new double[][] { Destination(latitude, longitude, 0.0, dist),
                                                 Destination(latitude, longitude, 90.0, dist),
                                                 Destination(latitude, longitude, 180.0, dist),
                                                 Destination(latitude, longitude, 270.0, dist)};

                //query db with bounds
                IList<BusStop> results = DatabaseHelper.QueryDatabase(bounds, location, dist, count);
                //IEnumerable<BusStop> filtered = FilterResults(results);

                //int counter = 0;
                //if (NearbyModel.nearbyStops != null) 
                //NearbyModel.nearbyStops.Clear();

                foreach (BusStop d in results)
                {
                    NearbyModel.nearbyStops.Add(new StopData(d.RouteTitle, d.Routes));
                    //d.Distance = Distance(latitude, longitude, d.Latitude, d.Longitude);
                }
            }
            else NearbyModel.nearbyStops.Add(new StopData("No Stops", ""));
        }

        private static double[] Destination(double lat, double lon, double bearing, double d)
        {
            double rLat = Deg2Rad(lat);
            double rLon = Deg2Rad(lon);
            double rBearing = Deg2Rad(bearing);
            double rDist = d / 3963.19;

            double rLatBound = Math.Asin(Math.Sin(rLat) * Math.Cos(rDist) + Math.Cos(rLat) * Math.Sin(rDist) * Math.Cos(rBearing));
            double rLonBound = rLon + Math.Atan2(Math.Sin(rBearing) * Math.Sin(rDist) * Math.Cos(rLat),
                                                 Math.Cos(rDist) - (Math.Sin(rLat) * Math.Sin(rLatBound)));

            double[] LatLon = new double[] { Rad2Deg(rLatBound), Rad2Deg(rLonBound) };
            return LatLon;
        }

        private static double Deg2Rad(double degrees)
        {
            return (Math.PI / 180) * degrees;
        }

        private static double Rad2Deg(double radians)
        {
            return (180 / Math.PI) * radians;
        }

        //private static IEnumerable<BusStop> FilterResults(IEnumerable<BusStop> list)
        //{
            
        //}

        private static double Distance(double latA, double lonA, double latB, double lonB)
        {
            double rLatA = Deg2Rad(latA);
            double rLatB = Deg2Rad(latB);
            double rHalfDeltaLat = Deg2Rad((latB - latA) / 2.0);
            double rHalfDeltaLon = Deg2Rad((lonB - lonA) / 2.0);

            return (2 * 3963.19) * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(rHalfDeltaLat), 2) + Math.Cos(rLatA) * Math.Cos(rLatB) * Math.Pow(Math.Sin(rHalfDeltaLon), 2)));

        }
    }
}
