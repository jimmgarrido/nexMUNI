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

namespace NexMuni
{
    public partial class MainPage : PhoneApplicationPage
    { 

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
            
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e) 
        {
            //SystemTray.ProgressIndicator = new ProgressIndicator();

            //UpdateMap();
     
        }

        private async void UpdateMap()
        {
            LocationText.Text="Getting location";
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            //SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "Getting location";

            Geoposition position =
                await geolocator.GetGeopositionAsync(
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(30));

            SystemTray.ProgressIndicator.Text = "Got it!";

            var gpsCoorCenter =
                new GeoCoordinate(
                    position.Coordinate.Latitude,
                    position.Coordinate.Longitude);

            StopMap.SetView(gpsCoorCenter, 17);
           // SetProgressIndicator(false);

            // Create a small circle to mark the current location.
            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Blue);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            // Create a MapOverlay to contain the circle.
            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = gpsCoorCenter;

            // Create a MapLayer to contain the MapOverlay.
            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);

            // Add the MapLayer to the Map.
            StopMap.Layers.Add(myLocationLayer);


            LocationText.Text = String.Format("{0} , {1}", position.Coordinate.Latitude, position.Coordinate.Longitude);
        }

        

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton searchButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
            searchButton.Text = AppResources.searchButtonText;
            ApplicationBar.Buttons.Add(searchButton);

            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        
    }
}


