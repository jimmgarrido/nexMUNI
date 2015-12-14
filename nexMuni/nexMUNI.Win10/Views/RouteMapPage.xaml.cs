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
            refreshTimer.Interval = new System.TimeSpan(0, 0, 20);
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

            //var inboundBus = new BitmapImage(new Uri("ms-appx:///Assets/Inbound.png"));
            //var outboundBus = new BitmapImage(new Uri("ms-appx:///Assets/Outbound.png"));

            //MapControl.SetNormalizedAnchorPoint(inboundBus, new Point(0.5, 0.5));
            //MapControl.SetNormalizedAnchorPoint(outboundBus, new Point(0.5, 0.5));

            //while (vehicleCounter > 0)
            //{
            //    RouteMap.Children.RemoveAt(RouteMap.Children.Count - 1);
            //    vehicleCounter--;
            //}

            //foreach (Bus bus in busLocations)
            //{
            //    //for (int i=3; i < RouteMap.Children.Count; i++)
            //    //{
            //    //    //RouteMap.Children[i];
            //    //    Bus mapBus = (Bus) RouteMap.Children.ElementAt(i);

            //    //    if(mapBus.busId == bus.busId)
            //    //    {
            //    //        MapControl.SetLocation(RouteMap.Children[i], new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
            //    //    }
            //    //}

            //    if (bus.direction.Equals("inbound"))
            //    {
            //        var busMarker = new Image
            //        {
            //            Source = inboundBus,
            //            Height = 20,
            //            Width = 20,

            //            RenderTransform = new RotateTransform { Angle = bus.busHeading },
            //            RenderTransformOrigin = new Point(0.5, 0.5)
            //        };
            //        MapControl.SetNormalizedAnchorPoint(busMarker, new Point(0.5, 0.5));
            //        MapControl.SetLocation(busMarker, new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
            //        RouteMap.Children.Add(busMarker);
            //    }
            //    else if (bus.direction.Equals("outbound"))
            //    {
            //        var busMarker = new Image
            //        {
            //            Source = outboundBus,
            //            Height = 20,
            //            Width = 20,

            //            RenderTransform = new RotateTransform { Angle = bus.busHeading },
            //            RenderTransformOrigin = new Point(0.5, 0.5)
            //        };
            //        MapControl.SetNormalizedAnchorPoint(busMarker, new Point(0.5, 0.5));
            //        MapControl.SetLocation(busMarker, new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
            //        RouteMap.Children.Add(busMarker);
            //    }
            //    vehicleCounter++;
            //}

            //var xmlDoc = await WebHelper.GetBusLocationsAsync(routeMapVm.SelectedRoute);
            //var vehicleLocations = await Task.Run(() => MapHelper.ParseBusLocations(xmlDoc));

            //var inboundBus = new BitmapImage();
            //inboundBus.DecodePixelHeight = 20;
            //inboundBus.UriSource = new Uri("ms-appx:///Assets/Inbound.png");

            //var outboundBus = new BitmapImage();
            //outboundBus.DecodePixelHeight = 20;
            //outboundBus.UriSource = new Uri("ms-appx:///Assets/Outbound.png");
            //outboundBus.

            //MapControl.SetNormalizedAnchorPoint(inboundBus, new Point(0.5, 0.5));
            //MapControl.SetNormalizedAnchorPoint(outboundBus, new Point(0.5, 0.5));

            foreach (Bus bus in busLocations)
            {
                if (bus.direction.Equals("inbound"))
                {
                    //var busM = new Image
                    //{
                    //    Source = inboundBus,
                    //    Height = 20,
                    //    Width = 20,

                    //    RenderTransform = new RotateTransform { Angle = bus.busHeading },
                    //    RenderTransformOrigin = new Point(0.5, 0.5)
                    //};
                    //busM.

                    var busMarker = new MapIcon
                    {
                        Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Inbound.png")),
                        CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
                        Location = new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }),
                        NormalizedAnchorPoint = new Point(0.5, 0.5),
                    };

                    //MapControl.SetNormalizedAnchorPoint(busMarker, new Point(0.5, 0.5));
                    //MapControl.SetLocation(busMarker, new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
                    RouteMap.MapElements.Add(busMarker);
                }
                else if (bus.direction.Equals("outbound"))
                {
                    //var busMarker = new Image
                    //{
                    //    Source = outboundBus,
                    //    Height = 20,
                    //    Width = 20,

                    //    RenderTransform = new RotateTransform { Angle = bus.busHeading },
                    //    RenderTransformOrigin = new Point(0.5, 0.5)
                    //};
                    var busMarker = new MapIcon
                    {
                        Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Outbound.png")),
                        CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
                        Location = new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }),
                        NormalizedAnchorPoint = new Point(0.5, 0.5),
                    };

                    //MapControl.SetNormalizedAnchorPoint(busMarker, new Point(0.5, 0.5));
                    //MapControl.SetLocation(busMarker, new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
                    RouteMap.MapElements.Add(busMarker);
                }
                vehicleCounter++;
            }
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
        }

        #endregion
    }
}
