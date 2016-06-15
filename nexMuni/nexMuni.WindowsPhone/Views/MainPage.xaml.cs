using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using nexMuni.DataModels;
using nexMuni.Helpers;
using nexMuni.ViewModels;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace nexMuni.Views
{
    public sealed partial class MainPage : Page
    {
        MainViewModel mainVm;
        SearchViewModel searchVm;

        bool alreadyLoaded;
        int vehicleCounter;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            MainPivot.SelectionChanged += PivotItemChanged;
            LocationHelper.LocationChanged += UpdateLocationOnMap;
            RouteBox.SelectionChanged += RouteSelected;
            DirBox.SelectionChanged += DirectionSelected;
            StopBox.SelectionChanged += StopSelected;

            MapControl.SetNormalizedAnchorPoint(LocationIcon, new Point(0.5, 0.5));
            MapControl.SetNormalizedAnchorPoint(StopIcon, new Point(0.5, 1.0));

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (alreadyLoaded) return;

            SettingsHelper.LoadNearbySetting();
            SettingsHelper.LoadLaunchPivotSetting();
            MainPivot.SelectedIndex = SettingsHelper.LaunchPivotIndex;

            mainVm = new MainViewModel();
            searchVm = new SearchViewModel();

            NearbyPivot.DataContext = mainVm;
            FavoritesPivot.DataContext = mainVm;
            SearchPivot.DataContext = searchVm;

            await DatabaseHelper.CheckDatabasesAsync();
            await searchVm.LoadRoutesAsync();
            RouteBox.IsEnabled = true;
            await mainVm.LoadAsync();

            alreadyLoaded = true;
        }


        /**
        ** Event Handlers
        **/
        private async void UpdateButtonPressed(object sender, RoutedEventArgs e)
        {
            RefreshBtn.IsEnabled = false;
            await mainVm.UpdateNearbyStopsAsync();
            RefreshBtn.IsEnabled = true;
        }

        private void SortFavorites(object sender, RoutedEventArgs e)
        {
           mainVm.SortFavorites();
        }

        private async void FavoriteSearch(object sender, RoutedEventArgs e)
        {
            await searchVm.FavoriteSearchAsync();
            FavoriteBtn.Click -= FavoriteSearch;
            FavoriteBtn.Click += UnfavoriteSearch;
            FavoriteBtn.Icon = new SymbolIcon(Symbol.Remove);
            FavoriteBtn.Label = "unfavorite";
        }

        private async void UnfavoriteSearch(object sender, RoutedEventArgs e)
        {
            await searchVm.UnfavoriteSearchAsync();
            FavoriteBtn.Click -= UnfavoriteSearch;
            FavoriteBtn.Click += FavoriteSearch;
            FavoriteBtn.Icon = new SymbolIcon(Symbol.Favorite);
            FavoriteBtn.Label = "favorite";
        }

        private async void RouteSelected(object sender, SelectionChangedEventArgs args)
        {
#if WINDOWS_PHONE_APP
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
            StatusBar.GetForCurrentView().ProgressIndicator.Text = "";
            await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
#endif
            DirBox.SelectedIndex = -1;
            StopBox.SelectedIndex = -1;
            await searchVm.LoadDirectionsAsync(((ComboBox)sender).SelectedItem as Route);

            DirLabel.Visibility = Visibility.Visible;
            DirBox.Visibility = Visibility.Visible;
            StopIcon.Visibility = Visibility.Collapsed;
            DirBox.SelectedIndex = 0;

            FavoriteBtn.IsEnabled = false;
            DetailBtn.IsEnabled = false;

            while (vehicleCounter > 0)
            {
                SearchMap.Children.RemoveAt(SearchMap.Children.Count - 1);
                vehicleCounter--;
            }

            SearchMap.MapElements.Clear();

            var routePath = await mainVm.GetRoutePathAsync(searchVm.SelectedRoute);

            SearchMap.MapElements.Clear();

            if (routePath.Any())
            {
                foreach (var points in routePath)
                {
                    SearchMap.MapElements.Add(new MapPolyline
                    {
                        Path = new Geopath(points),
                        StrokeColor = Color.FromArgb(255, 179, 27, 27),
                        StrokeThickness = 2.00,
                        ZIndex = 99
                    });
                }
            }

            await ShowVehicleLocations();
            await SearchMap.TrySetViewAsync(searchVm.MapCenter, 11.40);

#if WINDOWS_PHONE_APP
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = 0;
            await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
#endif
        }

        private async void DirectionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                StopBox.SelectedIndex = -1;
                await searchVm.LoadStops(((ComboBox)sender).SelectedItem.ToString());

                StopLabel.Visibility = Visibility.Visible;
                StopBox.Visibility = Visibility.Visible;
                StopIcon.Visibility = Visibility.Collapsed;
            }

            FavoriteBtn.IsEnabled = false;
            DetailBtn.IsEnabled = false;
        }

        private async void StopSelected(object sender, SelectionChangedEventArgs args)
        {
            if (((ComboBox)sender).SelectedIndex == -1) return;

#if WINDOWS_PHONE_APP
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
            StatusBar.GetForCurrentView().ProgressIndicator.Text = "Getting Arrival Times";
            await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
#endif
            await searchVm.StopSelectedAsync(((ComboBox)sender).SelectedItem as Stop);
            SearchTimes.Visibility = Visibility.Visible;

            if (searchVm.IsFavorite)
            {
                if (FavoriteBtn.Label == "favorite")
                {
                    FavoriteBtn.Click -= FavoriteSearch;
                    FavoriteBtn.Click += UnfavoriteSearch;
                    FavoriteBtn.Label = "unfavorite";
                    FavoriteBtn.Icon = new SymbolIcon(Symbol.Remove);
                }
            }
            else
            {
                if (FavoriteBtn.Label == "unfavorite")
                {
                    FavoriteBtn.Click -= UnfavoriteSearch;
                    FavoriteBtn.Click += FavoriteSearch;
                    FavoriteBtn.Label = "favorite";
                    FavoriteBtn.Icon = new SymbolIcon(Symbol.Favorite);
                }
            }
            FavoriteBtn.IsEnabled = true;
            DetailBtn.IsEnabled = true;

            await ShowStopLocation();

#if WINDOWS_PHONE_APP
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = 0;
            StatusBar.GetForCurrentView().ProgressIndicator.Text = "";
            await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
#endif
        }

        private void UpdateLocationOnMap()
        {
            RefreshBtn.IsEnabled = true;
            MapControl.SetNormalizedAnchorPoint(LocationIcon, new Point(0.5, 0.5));
            MapControl.SetLocation(LocationIcon, LocationHelper.Location.Coordinate.Point);
            mainVm.FavoritesDistances();
            SortBtn.IsEnabled = true;
        }

        private async Task AddRoutePath()
        {
            var routePath = await mainVm.GetRoutePathAsync(searchVm.SelectedRoute);

            if (routePath.Any())
            {
                foreach (var path in routePath)
                {
                    SearchMap.MapElements.Add(new MapPolyline
                    {
                        Path = new Geopath(path),
                        StrokeColor = Color.FromArgb(255, 179, 27, 27),
                        StrokeThickness = 2.00,
                        ZIndex = 99
                    });
                }
            }
        }

        private async Task ShowVehicleLocations()
        {
            var xmlDoc = await WebHelper.GetBusLocationsAsync(searchVm.SelectedRoute);
            var vehicleLocations = await Task.Run(() => MapHelper.ParseBusLocations(xmlDoc));

            var inboundBus = new BitmapImage();
            inboundBus.DecodePixelHeight = 20;
            inboundBus.UriSource = new Uri("ms-appx:///Assets/Inbound.png");

            var outboundBus = new BitmapImage();
            outboundBus.DecodePixelHeight = 20;
            outboundBus.UriSource = new Uri("ms-appx:///Assets/Outbound.png");

            MapControl.SetNormalizedAnchorPoint(inboundBus, new Point(0.5, 0.5));
            MapControl.SetNormalizedAnchorPoint(outboundBus, new Point(0.5, 0.5));

            foreach (Bus bus in vehicleLocations)
            {
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
                    SearchMap.Children.Add(busMarker);
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
                    SearchMap.Children.Add(busMarker);
                }
                vehicleCounter++;
            }
        }

        private async Task ShowStopLocation()
        {
            var stopLocation =  new Geopoint(new BasicGeoposition() { Latitude = searchVm.SelectedStop.Latitude, Longitude = searchVm.SelectedStop.Longitude });
            MapControl.SetNormalizedAnchorPoint(StopIcon, new Point(0.5, 1.0));
            MapControl.SetLocation(StopIcon, stopLocation);
            StopIcon.Visibility = Visibility.Visible;

            try {
                await SearchMap.TrySetViewAsync(stopLocation, 13.0);
            }
            catch(Exception ex)
            {
            }
        }

        private void StopSelected(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(StopDetail), e.ClickedItem);
        }

        private void DetailButtonPressed(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StopDetail), searchVm.SelectedStop);
        }

        private void GoToAbout(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

        private void GoToSettings(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void PivotItemChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((Pivot)sender).SelectedIndex)
            {
                case 0:
                    RefreshBtn.Visibility = Visibility.Visible;
                    SortBtn.Visibility = Visibility.Collapsed;
                    FavoriteBtn.Visibility = Visibility.Collapsed;
                    DetailBtn.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    SortBtn.Visibility = Visibility.Visible;
                    RefreshBtn.Visibility = Visibility.Collapsed;
                    FavoriteBtn.Visibility = Visibility.Collapsed;
                    DetailBtn.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    FavoriteBtn.Visibility = Visibility.Visible;
                    DetailBtn.Visibility = Visibility.Visible;
                    SortBtn.Visibility = Visibility.Collapsed;
                    RefreshBtn.Visibility = Visibility.Collapsed;
                    break;
            }
        }
    }
}
