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
using NexMuni.ViewModels;

namespace NexMuni
{
    public partial class MainPage : PhoneApplicationPage
    { 

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;

            DataContext = App.ViewModel;

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
            SystemTray.ProgressIndicator = new ProgressIndicator();
            nearbyList.ItemsSource = App.ViewModel.nearbyStops;
            UpdateLocation();
            
        }
        

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton searchButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
            searchButton.Text = AppResources.searchButtonText;
            ApplicationBar.Buttons.Add(searchButton);

            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        public async void UpdateLocation()
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


            var gpsCoorCenter = new GeoCoordinate(
                    position.Coordinate.Latitude,
                    position.Coordinate.Longitude);

            StopMap.SetView(gpsCoorCenter, 17);
            App.ViewModel.GetNearby(gpsCoorCenter);

            SetProgressIndicator(false);
        }

        private static void SetProgressIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }


        
    }
}


