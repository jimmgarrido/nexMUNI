using System;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using nexMuni.Common;
using nexMuni.DataModels;
using nexMuni.Helpers;
using nexMuni.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using Windows.Storage.Streams;
using Windows.UI;
using System.IO;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace nexMuni.Views
{
    public sealed partial class RouteMapPage : Page
    {
        private NavigationHelper navigationHelper;
        private bool alreadyLoaded;
        private DispatcherTimer refreshTimer;
        private int vehicleCounter = 0;

        public RouteMapViewModel routeMapVm;

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public RouteMapPage()
        {
            this.InitializeComponent(); 

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            RouteMap.Loaded += MapLoaded;

            refreshTimer = new DispatcherTimer();
            refreshTimer.Tick += TimerDue;
            refreshTimer.Interval = new TimeSpan(0, 0, 20);
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (alreadyLoaded) return;
            routeMapVm = new RouteMapViewModel(e.NavigationParameter as Route);
            DataContext = routeMapVm;

            RouteMap.Center= routeMapVm.SelectedRoute.stopLocation;
            MapControl.SetNormalizedAnchorPoint(StopIcon, new Point(0.5, 1.0));
            MapControl.SetNormalizedAnchorPoint(LocationIcon, new Point(0.5, 0.5));

            if (LocationHelper.Location != null)
            {
                MapControl.SetLocation(LocationIcon, LocationHelper.Point);
                LocationIcon.Visibility = Visibility.Visible;
            }

            if (!refreshTimer.IsEnabled) refreshTimer.Start();
            alreadyLoaded = true;
        }

        private async Task ShowRoutePath()
        {
            var routePath = await routeMapVm.GetRoutePath();
            if (routePath.Any())
            {
                foreach (var points in routePath)
                {
                    RouteMap.MapElements.Add(new MapPolyline
                    {
                        Path = new Geopath(points),
                        StrokeColor = Color.FromArgb(255, 179, 27, 27),
                        StrokeThickness = 2.00
                    });
                }
            }
        }

        private async Task AddVehicleLocations()
        {
            var busLocations = await routeMapVm.GetBusLocations();

            while (vehicleCounter > 0)
            {
                RouteMap.MapElements.RemoveAt(RouteMap.MapElements.Count - 1);
                vehicleCounter--;
            }

            var inboundBM = new WriteableBitmap(48, 48);
            await inboundBM.SetSourceAsync(await RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Inbound.png")).OpenReadAsync());

            var outboundBM = new WriteableBitmap(48, 48);
            await outboundBM.SetSourceAsync(await RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Outbound.png")).OpenReadAsync());

            foreach (Bus bus in busLocations)
            {
                if (bus.direction.Equals("inbound"))
                {
                    var rotatedImage = inboundBM.RotateFree(bus.busHeading, false);
                    var stream = new InMemoryRandomAccessStream();
                    await rotatedImage.ToStream(stream, BitmapEncoder.PngEncoderId);

                    var busMarker = new MapIcon
                    {
                        Image = RandomAccessStreamReference.CreateFromStream(stream),
                        CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
                        Location = new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }),
                        NormalizedAnchorPoint = new Point(0.5, 0.5),
                        ZIndex = 99
                    };

                    RouteMap.MapElements.Add(busMarker);

                    await stream.FlushAsync();
                    stream.Dispose();
                    rotatedImage = null;
                }
                else if (bus.direction.Equals("outbound"))
                {
                    var rotatedImage = outboundBM.RotateFree(bus.busHeading, false);
                    var stream = new InMemoryRandomAccessStream();
                    await rotatedImage.ToStream(stream, BitmapEncoder.PngEncoderId);

                    var busMarker = new MapIcon
                    {
                        Image = RandomAccessStreamReference.CreateFromStream(stream),
                        CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
                        Location = new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }),
                        NormalizedAnchorPoint = new Point(0.5, 0.5),
                        ZIndex = 99
                    };

                    RouteMap.MapElements.Add(busMarker);

                    await stream.FlushAsync();
                    stream.Dispose();
                    rotatedImage = null;
                }
                vehicleCounter++;
            }

            inboundBM = null;
            outboundBM = null;
        }

        private async void MapLoaded(object sender, RoutedEventArgs e)
        {
            MapControl.SetLocation(StopIcon, routeMapVm.SelectedRoute.stopLocation);
            StopIcon.Visibility = Visibility.Visible;

            await ShowRoutePath();
            await AddVehicleLocations();
        }

        private async void TimerDue(object sender, object e)
        {
            await AddVehicleLocations();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
            refreshTimer.Stop();
            refreshTimer.Tick -= TimerDue;
        }

        #endregion
    }
}
