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

namespace nexMuni.Views
{
    public sealed partial class RouteMapPage : Page
    {
        private NavigationHelper navigationHelper;
        private bool alreadyLoaded;
        private DispatcherTimer refreshTimer;
        private int vehicleCounter;

        public RouteMapViewModel routeMapVm;

        public RouteMapPage()
        {
            this.InitializeComponent(); 

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            refreshTimer = new DispatcherTimer();
            refreshTimer.Tick += TimerDue;
            refreshTimer.Interval = new System.TimeSpan(0, 0, 30);
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (alreadyLoaded) return;
            routeMapVm = new RouteMapViewModel(e.NavigationParameter as Route);
            DataContext = routeMapVm;

            RouteMap.Center= routeMapVm.SelectedRoute.stopLocation;
            MapControl.SetNormalizedAnchorPoint(StopIcon, new Point(0.5, 1.0));
            MapControl.SetNormalizedAnchorPoint(LocationIcon, new Point(0.5, 0.5));

            MapControl.SetLocation(StopIcon, routeMapVm.SelectedRoute.stopLocation);
            StopIcon.Visibility = Visibility.Visible;
            if (LocationHelper.Location != null)
            {
                MapControl.SetLocation(LocationIcon, LocationHelper.Point);
                LocationIcon.Visibility = Visibility.Visible;
            }

            var routePath = await routeMapVm.GetRoutePath();


            await AddVehicleLocations();
            if (!refreshTimer.IsEnabled) refreshTimer.Start();
            alreadyLoaded = true;
        }

        private async Task AddVehicleLocations()
        {
            var busLocations = await routeMapVm.GetBusLocations();

            var inboundBus = new BitmapImage(new Uri("ms-appx:///Assets/Inbound.png"));
            var outboundBus = new BitmapImage(new Uri("ms-appx:///Assets/Outbound.png"));

            MapControl.SetNormalizedAnchorPoint(inboundBus, new Point(0.5, 0.5));
            MapControl.SetNormalizedAnchorPoint(outboundBus, new Point(0.5, 0.5));

            while (vehicleCounter > 0)
            {
                RouteMap.Children.RemoveAt(RouteMap.Children.Count - 1);
                vehicleCounter--;
            }

            foreach (Bus bus in busLocations)
            {
                //for (int i=3; i < RouteMap.Children.Count; i++)
                //{
                //    //RouteMap.Children[i];
                //    Bus mapBus = (Bus) RouteMap.Children.ElementAt(i);
                    
                //    if(mapBus.busId == bus.busId)
                //    {
                //        MapControl.SetLocation(RouteMap.Children[i], new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
                //    }
                //}

                if (bus.direction.Equals("inbound"))
                {
                    var busMarker = new Image
                    {
                        Source = inboundBus,
                        Height = 20,
                        Width = 20,

                        RenderTransform = new RotateTransform { Angle = bus.busHeading },
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };
                    MapControl.SetNormalizedAnchorPoint(busMarker, new Point(0.5, 0.5));
                    MapControl.SetLocation(busMarker, new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
                    RouteMap.Children.Add(busMarker);
                }
                else if (bus.direction.Equals("outbound"))
                {
                    var busMarker = new Image
                    {
                        Source = outboundBus,
                        Height = 20,
                        Width = 20,

                        RenderTransform = new RotateTransform { Angle = bus.busHeading },
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };
                    MapControl.SetNormalizedAnchorPoint(busMarker, new Point(0.5, 0.5));
                    MapControl.SetLocation(busMarker, new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
                    RouteMap.Children.Add(busMarker);
                }
                vehicleCounter++;
            }
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
        }

        #endregion
    }
}
