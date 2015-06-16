using nexMuni.Common;
using nexMuni.DataModels;
using nexMuni.Helpers;
using nexMuni.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;

namespace nexMuni.Views
{
    public sealed partial class RouteMapPage : Page
    {
        //private Route selectedRoute;
        private List<MapPolyline> routePath;
        private NavigationHelper navigationHelper;
        private bool alreadyLoaded;

        //public Geopoint Center {
        //    get 
        //    {
        //        return Center;
        //    }
        //    set
        //    {
        //        new BasicGeoposition() { Latitude = 37.7603, Longitude = -122.427 };
        //    }
        //}

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

                //routeTitle.Text = selectedRoute.RouteNumber + "-" + selectedRoute.RouteName;
                RouteMap.Center = new Geopoint(new BasicGeoposition() { Latitude = 37.7603, Longitude = -122.427 });
                MapControl.SetLocation(LocationIcon, LocationHelper.Location.Coordinate.Point);
                LocationIcon.Visibility = Windows.UI.Xaml.Visibility.Visible;

                routePath = await routeMapVm.GetRoutePath();
                //routePath = await MapHelper.LoadDoc(selectedRoute.RouteNumber);
                foreach (MapPolyline line in routePath)
                {
                    RouteMap.MapElements.Add(line);
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
