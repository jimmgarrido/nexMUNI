using nexMuni.Common;
using nexMuni.DataModels;
using nexMuni.Helpers;
using nexMuni.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace nexMuni.Views
{
    public sealed partial class RouteMapPage : Page
    {
        private NavigationHelper navigationHelper;
        private bool alreadyLoaded;

        public RouteMapViewModel routeMapVm;

        public RouteMapPage()
        {
            this.InitializeComponent(); 

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (!alreadyLoaded)
            {
                routeMapVm = new RouteMapViewModel(e.NavigationParameter as Route);
                DataContext = routeMapVm;

                RouteMap.Center= routeMapVm.SelectedRoute.stopLocation;
                MapControl.SetNormalizedAnchorPoint(StopIcon, new Windows.Foundation.Point(0.5, 1.0));
                MapControl.SetNormalizedAnchorPoint(LocationIcon, new Windows.Foundation.Point(0.5, 0.5));

                MapControl.SetLocation(StopIcon, routeMapVm.SelectedRoute.stopLocation);
                StopIcon.Visibility = Windows.UI.Xaml.Visibility.Visible;
                if (LocationHelper.Location != null)
                {
                    MapControl.SetLocation(LocationIcon, LocationHelper.Point);
                    LocationIcon.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }

                var routePath = await routeMapVm.GetRoutePath();
                var busLocations = await routeMapVm.GetBusLocations();

                var inboundBus = new BitmapImage(new Uri("ms-appx:///Assets/Inbound.png"));
                var outboundBus = new BitmapImage(new Uri("ms-appx:///Assets/Outbound.png"));

                MapControl.SetNormalizedAnchorPoint(inboundBus, new Windows.Foundation.Point(0.5, 0.5));
                MapControl.SetNormalizedAnchorPoint(outboundBus, new Windows.Foundation.Point(0.5, 0.5));

                foreach (MapPolyline line in routePath)
                {
                    RouteMap.MapElements.Add(line);
                }

                foreach(Bus bus in busLocations)
                {
                    if (bus.direction.Equals("inbound"))
                    {
                        var busMarker = new Image
                        {
                            Source = inboundBus,
                            Height = 20,
                            Width = 20,

                            RenderTransform = new RotateTransform { Angle = bus.busHeading },
                            RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5)
                        };
                        MapControl.SetNormalizedAnchorPoint(busMarker, new Windows.Foundation.Point(0.5, 0.5));
                        MapControl.SetLocation(busMarker, new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
                        RouteMap.Children.Add(busMarker);
                        busMarker = null;
                    }
                    else if (bus.direction.Equals("outbound"))
                    {
                        var busMarker = new Image
                        {
                            Source = outboundBus,
                            Height = 20,
                            Width = 20,

                            RenderTransform = new RotateTransform { Angle = bus.busHeading },
                            RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5)
                        };
                        MapControl.SetNormalizedAnchorPoint(busMarker, new Windows.Foundation.Point(0.5, 0.5));
                        MapControl.SetLocation(busMarker, new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
                        RouteMap.Children.Add(busMarker);
                        busMarker = null;
                    }
                }
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
