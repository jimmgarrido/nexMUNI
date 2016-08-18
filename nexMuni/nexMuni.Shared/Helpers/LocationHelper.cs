using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.ViewManagement;
using nexMuni.DataModels;

namespace nexMuni.Helpers
{
    public class LocationHelper
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
            await UIHelper.ShowStatusBar("Getting Location");

#if !WINDOWS_PHONE_APP
            var accessStatus = await Geolocator.RequestAccessAsync();

            switch(accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    if (geolocator == null) geolocator = new Geolocator { DesiredAccuracyInMeters = 50 };
                    Location = await geolocator.GetGeopositionAsync(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
                    latitude = Point.Position.Latitude;
                    longitude = Point.Position.Longitude;
                    LocationChanged?.Invoke();
                    break;                    
            }
#else

            if (geolocator == null) geolocator = new Geolocator { DesiredAccuracyInMeters = 50 };
            if (geolocator.LocationStatus == PositionStatus.Disabled)
            {
                //MainPage.noNearbyText.Text = "Location services disabled";
            }
            else
            {
                Location = await geolocator.GetGeopositionAsync(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
                latitude = Point.Position.Latitude;
                longitude = Point.Position.Longitude;
                LocationChanged?.Invoke();
            }
#endif

            await UIHelper.HideStatusBar();
        }

        public static double[][] MakeBounds(double dist)
        {
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

            double[] latLon = { Rad2Deg(rLatBound), Rad2Deg(rLonBound) };
            return latLon;
        }

        public static double GetDistance(double latB, double lonB)
        {
            double rLatA = Deg2Rad(latitude);
            double rLatB = Deg2Rad(latB);
            double rHalfDeltaLat = Deg2Rad((latB - latitude) / 2.0);
            double rHalfDeltaLon = Deg2Rad((lonB - longitude) / 2.0);

            return (2 * 3963.19) * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(rHalfDeltaLat), 2) + Math.Cos(rLatA) * Math.Cos(rLatB) * Math.Pow(Math.Sin(rHalfDeltaLon), 2)));
        }

        public static async void FavoritesDistances(ObservableCollection<Stop> favorites)
        {
            if (Location == null) return;
            foreach (Stop stop in favorites)
            {
                stop.DistanceAsDouble = await Task.Run(()=> GetDistance(stop.Latitude, stop.Longitude));
            }
        }

        private static double Deg2Rad(double degrees)
        {
            return (Math.PI / 180) * degrees;
        }

        private static double Rad2Deg(double radians)
        {
            return (180 / Math.PI) * radians;
        }
    }
}
