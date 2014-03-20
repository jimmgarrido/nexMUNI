using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NexMuni.Resources;
using System.Device.Location;
using Windows.Devices.Geolocation;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Phone.Maps.Controls;

namespace NexMuni.ViewModels
{
    public class MainPageModel
    {
        public StopGroup nearbyStops { get; set; }
        public bool IsDataLoaded { get; set; }

        public void LoadData()
        {
            //Load data
            nearbyStops = CreateStopGroup();

            IsDataLoaded = true;

        }

        private StopGroup CreateStopGroup()
        {
            StopGroup stopData = new StopGroup();
            SystemTray.ProgressIndicator = new ProgressIndicator();

            UpdateNearby(stopData);

            return stopData;
        }

        private async void UpdateNearby(StopGroup group)
        {
            //Get the user's location coordinates
            SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "Getting location";
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;


            Geoposition position =
                await geolocator.GetGeopositionAsync(
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(30));

            SystemTray.ProgressIndicator.Text = "Got it!";


            var gpsCoorCenter =
                new GeoCoordinate(
                    position.Coordinate.Latitude,
                    position.Coordinate.Longitude);
            SetProgressIndicator(false);
            
            //Open Muni Stops db
            StopsDbDataContext muniStops = new StopsDbDataContext(StopsDbDataContext.DBConnectionString);

            int index = 0;

            foreach (Stops stopCorner in muniStops.stopsTable)
            {
                if (index < 10)
                {
                    //cornersArray[index] = stopCorner.Stop_name;
                    group.nearbyStopsList.Add(new StopData { StopName = stopCorner.Stop_name });
                    index++;
                }
                else
                    break;
                
            }

        }

        private static void SetProgressIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }

    }

    [Table(Name = "stops")]
    public partial class Stops
    {

        private System.Nullable<long> __id;

        private string _Stop_name;

        private System.Nullable<double> _Stop_lat;

        private System.Nullable<double> _Stop_lon;

        private string _Stop_id;

        private string _Stop_tags;

        public Stops()
        {
        }

        [Column(Storage = "__id", DbType = "BigInt")]
        public System.Nullable<long> _id
        {
            get
            {
                return this.__id;
            }
            set
            {
                if ((this.__id != value))
                {
                    this.__id = value;
                }
            }
        }

        [Column(Name = "stop_name", Storage = "_Stop_name", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
        public string Stop_name
        {
            get
            {
                return this._Stop_name;
            }
            set
            {
                if ((this._Stop_name != value))
                {
                    this._Stop_name = value;
                }
            }
        }

        [Column(Name = "stop_lat", Storage = "_Stop_lat", DbType = "Float")]
        public System.Nullable<double> Stop_lat
        {
            get
            {
                return this._Stop_lat;
            }
            set
            {
                if ((this._Stop_lat != value))
                {
                    this._Stop_lat = value;
                }
            }
        }

        [Column(Name = "stop_lon", Storage = "_Stop_lon", DbType = "Float")]
        public System.Nullable<double> Stop_lon
        {
            get
            {
                return this._Stop_lon;
            }
            set
            {
                if ((this._Stop_lon != value))
                {
                    this._Stop_lon = value;
                }
            }
        }

        [Column(Name = "stop_id", Storage = "_Stop_id", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
        public string Stop_id
        {
            get
            {
                return this._Stop_id;
            }
            set
            {
                if ((this._Stop_id != value))
                {
                    this._Stop_id = value;
                }
            }
        }

        [Column(Name = "stop_tags", Storage = "_Stop_tags", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
        public string Stop_tags
        {
            get
            {
                return this._Stop_tags;
            }
            set
            {
                if ((this._Stop_tags != value))
                {
                    this._Stop_tags = value;
                }
            }
        }
    }

    public class StopsDbDataContext : DataContext
    {
        // Specify the connection string as a static, used in main page and app.xaml.
        public static string DBConnectionString = "Data Source=appdata:Resources/NexMuni.sdf";

        // Pass the connection string to the base class.
        public StopsDbDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify a single table for the to-do items.
        public Table<Stops> stopsTable;
    }

    
}
