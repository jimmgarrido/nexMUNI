using nexMuni.DataModels;
using nexMuni.Helpers;
using nexMuni.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;

namespace nexMuni.Views
{
    public sealed partial class MainPage : Page
    {
        MainViewModel mainVm;
        SearchViewModel searchVm;

        bool alreadyLoaded;
        int vehicleCounter = 0;
        DispatcherTimer refreshTimer;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            MainPivot.SelectionChanged += PivotItemChanged;
            LocationHelper.LocationChanged += UpdateLocationOnMap;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!alreadyLoaded)
            {
                mainVm = new MainViewModel();
                searchVm = new SearchViewModel();
                //searchVm.UpdateLocation += LocationUpdated;

                NearbyPivot.DataContext = mainVm;
                FavoritesPivot.DataContext = mainVm;
                SearchPivot.DataContext = searchVm;

                RouteBox.SelectionChanged += RouteSelected;
                DirBox.SelectionChanged += DirectionSelected;
                StopBox.SelectionChanged += StopSelected;

                SettingsHelper.LoadNearbySetting();
                SettingsHelper.LoadLaunchPivotSetting();
                MainPivot.SelectedIndex = SettingsHelper.launchPivot;

                await DatabaseHelper.CheckDatabasesAsync();
                await searchVm.LoadRoutesAsync();
                RouteBox.IsEnabled = true;
                await mainVm.LoadAsync();
                LoadingRing.IsActive = false;

                alreadyLoaded = true;
            }
        }

        private async void RouteSelected(object sender, SelectionChangedEventArgs args)
        {
            await UIHelper.ShowStatusBar("Getting Route Info...");

            try {
                DirBox.SelectedIndex = -1;
                StopBox.SelectedIndex = -1;
                await searchVm.LoadDirectionsAsync(((ComboBox)sender).SelectedItem.ToString());

                DirBox.Visibility = Visibility.Visible;
                DirBox.SelectedIndex = 0;

                FavoriteBtn.IsEnabled = false;
                DetailBtn.IsEnabled = false;

                await SearchMap.TrySetViewAsync(searchVm.MapCenter, 11.40);
                await ShowRoutePath();
                await ShowVehicleLocations();

                if (refreshTimer == null)
                {
                    refreshTimer = new DispatcherTimer();
                    refreshTimer.Tick += TimerDue;
                    refreshTimer.Interval = new TimeSpan(0, 0, 20);
                }

                if (!refreshTimer.IsEnabled) refreshTimer.Start();
            }
            catch (Exception)
            {

            }

            await UIHelper.HideStatusBar();
        }

        private async void DirectionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                StopBox.SelectedIndex = -1;
                await searchVm.LoadStops(((ComboBox)sender).SelectedItem.ToString());

                //StopLabel.Visibility = Visibility.Visible;
                StopBox.Visibility = Visibility.Visible;
                //StopIcon.Visibility = Visibility.Collapsed;
            }

            FavoriteBtn.IsEnabled = false;
            DetailBtn.IsEnabled = false;
        }

        private async void StopSelected(object sender, SelectionChangedEventArgs args)
        {
            if (((ComboBox)sender).SelectedIndex == -1) return;

            await searchVm.StopSelectedAsync(((ComboBox)sender).SelectedItem as Stop);
            SearchTimes.Visibility = Visibility.Visible;

            if (searchVm.IsFavorite())
            {
                if (FavoriteBtn.Label == "favorite") FavoriteBtn.Click -= FavoriteSearch;
                FavoriteBtn.Click += UnfavoriteSearch;
                FavoriteBtn.Label = "unfavorite";
                FavoriteBtn.Icon = new SymbolIcon(Symbol.Remove);
            }
            else
            {
                if (FavoriteBtn.Label == "unfavorite") FavoriteBtn.Click -= UnfavoriteSearch;
                FavoriteBtn.Click += FavoriteSearch;
                FavoriteBtn.Label = "favorite";
                FavoriteBtn.Icon = new SymbolIcon(Symbol.Favorite);
            }
            FavoriteBtn.IsEnabled = true;
            DetailBtn.IsEnabled = true;

            await ShowStopLocation();
        }

        private async Task ShowRoutePath()
        {
            SearchMap.MapElements.Clear();

            var routePath = await mainVm.GetRoutePathAsync(searchVm.SelectedRoute);
            if (routePath.Any())
            {
                foreach (var points in routePath)
                {
                    var temp = new MapPolyline();
                    temp.StrokeColor = Color.FromArgb(255, 179, 27, 27);
                    temp.StrokeThickness = 2.0;
                    temp.ZIndex = 99;
                    temp.Path = new Geopath(points);

                    //var temp = new MapPolyline
                    //{
                    //    Path = new Geopath(points),
                    //    StrokeColor = Color.FromArgb(255, 179, 27, 27),
                    //    StrokeThickness = 2.00,
                    //    ZIndex = 99
                    //};
                    SearchMap.MapElements.Add(temp);
                }
            }
        }

        private async Task ShowVehicleLocations()
        {
            var xmlDoc = await WebHelper.GetBusLocationsAsync(searchVm.SelectedRoute);
            var vehicleLocations = await Task.Run(() => MapHelper.ParseBusLocations(xmlDoc));

            while (vehicleCounter > 0)
            {
                SearchMap.MapElements.RemoveAt(SearchMap.MapElements.Count - 1);
                vehicleCounter--;
            }

            foreach (Bus bus in vehicleLocations)
            {
                if (bus.direction.Equals("inbound"))
                {
                    //var busMarker = new Image
                    //{
                    //    Source = inboundBus,
                    //    Height = 20,
                    //    Width = 20,

                    //    RenderTransform = new RotateTransform { Angle = bus.busHeading },
                    //    RenderTransformOrigin = new Point(0.5, 0.5)
                    //};
                    var busMarker = new MapIcon
                    {
                        Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Inbound.png")),
                        CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
                        Location = new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }),
                        NormalizedAnchorPoint = new Point(0.5, 0.5),
                    };
                    
                    //MapControl.SetNormalizedAnchorPoint(busMarker, new Point(0.5, 0.5));
                    //MapControl.SetLocation(busMarker, new Geopoint(new BasicGeoposition { Latitude = bus.latitude, Longitude = bus.longitude }));
                    SearchMap.MapElements.Add(busMarker);
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
                    SearchMap.MapElements.Add(busMarker);
                }
                vehicleCounter++;
            }
        }

        private async Task ShowStopLocation()
        {
            //var telemetry = new TelemetryClient();
            var stopLocation = new Geopoint(new BasicGeoposition() { Latitude = searchVm.SelectedStop.Latitude, Longitude = searchVm.SelectedStop.Longitude });

            //var stopImage = new BitmapImage();
            //stopImage.DecodePixelHeight = 30;
            //stopImage.UriSource = new Uri("ms-appx:///Assets/Stop.png");
            var StopIcon = new MapIcon
            {
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Stop.png")),
                CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
                Location = stopLocation,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                ZIndex = 99
            };
   
            MapControl.SetNormalizedAnchorPoint(StopIcon, new Point(0.5, 1.0));
            //MapControl.SetLocation(StopIcon, stopLocation);
            //StopIcon.Visibility = Visibility.Visible;
            try
            {
                SearchMap.MapElements.Add(StopIcon);
                await SearchMap.TrySetViewAsync(stopLocation, 13.0);
            }
            catch (Exception ex)
            {
                //telemetry.TrackException(ex);
            }
        }

        private void UpdateLocationOnMap()
        {
            RefreshBtn.IsEnabled = true;
            MapControl.SetNormalizedAnchorPoint(LocationIcon, new Point(0.5, 0.5));
            MapControl.SetLocation(LocationIcon, LocationHelper.Location.Coordinate.Point);
            LocationIcon.Visibility = Visibility.Visible;
            mainVm.FavoritesDistances();
            SortBtn.IsEnabled = true;
        }

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
            FavoriteBtn.Icon = new SymbolIcon(Symbol.UnFavorite);
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
        private void StopClicked(object sender, ItemClickEventArgs e)
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

        private async void TimerDue(object sender, object e)
        {
            await ShowVehicleLocations();
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if(refreshTimer != null && refreshTimer.IsEnabled)
                refreshTimer.Stop();
        }
    }
}
