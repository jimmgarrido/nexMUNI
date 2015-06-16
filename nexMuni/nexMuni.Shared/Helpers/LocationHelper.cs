using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using nexMuni.ViewModels;
using nexMuni.Views;
using nexMuni.DataModels;

namespace nexMuni.Helpers
{
    class LocationHelper
    {
        public static Geoposition Location { get; private set; }
        public static Geopoint Point
        {
            get
            {
                return Location.Coordinate.Point;
            }
        }
        public static ChangedEventHandler LocationChanged;

        private static double latitude;
        private static double longitude;
        private static Geolocator geolocator;

        public static async Task UpdateLocation()
        {
#if WINDOWS_PHONE_APP
            var statusBar = StatusBar.GetForCurrentView();
            await statusBar.ProgressIndicator.ShowAsync();
            statusBar.ProgressIndicator.Text = "Getting Location";
            statusBar.ProgressIndicator.ProgressValue = null;
#endif
            if(geolocator == null) geolocator = new Geolocator { DesiredAccuracyInMeters = 50 };

            if (geolocator.LocationStatus == PositionStatus.Disabled)
            {
                //MainPage.noNearbyText.Text = "Location services disabled";
            }
            else
            {
                Location = await geolocator.GetGeopositionAsync(maximumAge: TimeSpan.FromSeconds(10), timeout: TimeSpan.FromSeconds(30));
                if (LocationChanged != null) LocationChanged();
            }

#if WINDOWS_PHONE_APP
            statusBar.ProgressIndicator.ProgressValue = 0;
            await statusBar.ProgressIndicator.HideAsync();
#endif
        }

        public static double[][] MakeBounds(double dist)
        {
            latitude = Point.Position.Latitude;
            longitude = Point.Position.Longitude;

            //Create search radius bounds
           return new double[][] { Destination(latitude, longitude, 0.0, dist),
                                                Destination(latitude, longitude, 90.0, dist),
                                                Destination(latitude, longitude, 180.0, dist),
                                                Destination(latitude, longitude, 270.0, dist)};
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

        public static double GetDistance(double latB, double lonB)
        {
            double rLatA = Deg2Rad(latitude);
            double rLatB = Deg2Rad(latB);
            double rHalfDeltaLat = Deg2Rad((latB - latitude) / 2.0);
            double rHalfDeltaLon = Deg2Rad((lonB - longitude) / 2.0);

            return (2 * 3963.19) * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(rHalfDeltaLat), 2) + Math.Cos(rLatA) * Math.Cos(rLatB) * Math.Pow(Math.Sin(rHalfDeltaLon), 2)));

        }

        internal static void SortFavorites()
        {
            //if (Location != null)
            //{
            //    FavoritesDistance();

            //    var tempCollection = new ObservableCollection<Stop>(MainViewModel.FavoritesStops.OrderBy(z => z.DoubleDist));

            //    MainViewModel.FavoritesStops.Clear();
            //    foreach (StopData s in tempCollection)
            //    {
            //        MainViewModel.FavoritesStops.Add(new StopData(s.Name, s.Routes, s.Tags, s.DoubleDist, s.Lat, s.Lon, s.FavID));
            //    }
            //}
        }

        public static void FavoritesDistance()
        {
            //foreach (StopData stop in MainViewModel.FavoritesStops)
            //{
            //    stop.DoubleDist = GetDistance(stop.Lat, stop.Lon);
            //}
        }
    }
}
