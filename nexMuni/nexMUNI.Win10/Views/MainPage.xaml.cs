using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using nexMuni.DataModels;
using nexMuni.Helpers;
using nexMuni.ViewModels;

namespace nexMuni.Views
{
    public sealed partial class MainPage : Page
    {
        MainViewModel mainVm;
        SearchViewModel searchVm;

        bool alreadyLoaded = false, hasSearched= false;
        int vehicleCounter = 0;
        DispatcherTimer refreshTimer;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            mainVm = new MainViewModel();
            searchVm = new SearchViewModel();

            NearbyPivot.DataContext = mainVm;
            FavoritesPivot.DataContext = mainVm;
            SearchPivot.DataContext = searchVm;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Hook up event handlers
            MainPivot.SelectionChanged += PivotItemChanged;
            LocationHelper.LocationChanged += UpdateLocationOnMap;
            RouteBox.SelectionChanged += RouteSelected;
            DirBox.SelectionChanged += DirectionSelected;
            StopBox.SelectionChanged += StopSelected;

            if (!alreadyLoaded)
            {
                //await mainVm.Init();
                //await searchVm.LoadRoutesAsync();
                   
                MainPivot.SelectedIndex = SettingsHelper.LaunchPivotIndex;

                SettingsHelper.LoadSettings();
                await DatabaseHelper.CheckDatabasesAsync();
                await searchVm.LoadRoutesAsync();
                await mainVm.LoadAsync();

                RouteBox.IsEnabled = true;
                LoadingRing.IsActive = false;

                alreadyLoaded = true;
            }
        }

        private async void RouteSelected(object sender, SelectionChangedEventArgs args)
        {
            await UIHelper.ShowStatusBar("Getting Route Info");

            try {
                DirBox.SelectedIndex = -1;
                StopBox.SelectedIndex = -1;
                await searchVm.LoadDirectionsAsync(((ComboBox)sender).SelectedItem as Route);

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

                StopBox.Visibility = Visibility.Visible;
            }

            FavoriteBtn.IsEnabled = false;
            DetailBtn.IsEnabled = false;
        }

        private async void StopSelected(object sender, SelectionChangedEventArgs args)
        {
            if (((ComboBox)sender).SelectedIndex == -1) return;
            await UIHelper.ShowStatusBar("Getting Arrival Times");

            try
            {
                await searchVm.StopSelectedAsync(((ComboBox)sender).SelectedItem as Stop);
                SearchTimes.Visibility = Visibility.Visible;

                if (searchVm.IsFavorite)
                {
                    if (FavoriteBtn.Label == "favorite")
                    {
                        FavoriteBtn.Click -= FavoriteSearch;
                    }

                    FavoriteBtn.Click += UnfavoriteSearch;
                    FavoriteBtn.Label = "unfavorite";
                    FavoriteBtn.Icon = new SymbolIcon(Symbol.UnFavorite);
                }
                else
                {
                    if (FavoriteBtn.Label == "unfavorite")
                    {
                        FavoriteBtn.Click -= UnfavoriteSearch;
                    }

                    FavoriteBtn.Label = "favorite";
                    FavoriteBtn.Icon = new SymbolIcon(Symbol.Favorite);
                    FavoriteBtn.Click += FavoriteSearch;
                }

                FavoriteBtn.Visibility = Visibility.Visible;
                DetailBtn.Visibility = Visibility.Visible;
                FavoriteBtn.IsEnabled = true;
                DetailBtn.IsEnabled = true;

                hasSearched = true;
                await ShowStopLocation();
            }
            catch(Exception)
            {

            }

            await UIHelper.HideStatusBar();
        }

        private async Task ShowRoutePath()
        {
            SearchMap.MapElements.Clear();
            vehicleCounter = 0;

            var routePath = await mainVm.GetRoutePathAsync(searchVm.SelectedRoute);
            if (routePath.Any())
            {
                foreach (var points in routePath)
                {
                    var temp = new MapPolyline();
                    temp.StrokeColor = Color.FromArgb(255, 179, 27, 27);
                    temp.StrokeThickness = 2.0;
                    temp.ZIndex = 50;
                    temp.Path = new Geopath(points);

                    SearchMap.MapElements.Add(temp);
                }

                var StopIcon = new MapIcon
                {
                    Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Stop.png")),
                    CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
                    NormalizedAnchorPoint = new Point(0.5, 1.0),
                    ZIndex = 99
                };

                SearchMap.MapElements.Insert(0,StopIcon);
            }
        }

        private async Task ShowVehicleLocations()
        {
            var vehicleLocations = await searchVm.GetBusLocations();

            while (vehicleCounter > 0)
            {
                SearchMap.MapElements.RemoveAt(SearchMap.MapElements.Count - 1);
                vehicleCounter--;
            }

            var inboundBM = new WriteableBitmap(40, 40);
            await inboundBM.SetSourceAsync(await RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Inbound.png")).OpenReadAsync());

            var outboundBM = new WriteableBitmap(40, 40);
            await outboundBM.SetSourceAsync(await RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Outbound.png")).OpenReadAsync());

            foreach (Bus bus in vehicleLocations)
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

                    SearchMap.MapElements.Add(busMarker);

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

                    SearchMap.MapElements.Add(busMarker);

                    await stream.FlushAsync();
                    stream.Dispose();
                    rotatedImage = null;
                }
                vehicleCounter++;
            }

            inboundBM = null;
            outboundBM = null;
        }

        private async Task ShowStopLocation()
        {
            //var telemetry = new TelemetryClient();
            var stopLocation = new Geopoint(new BasicGeoposition() { Latitude = searchVm.SelectedStop.Latitude, Longitude = searchVm.SelectedStop.Longitude });

            //var stopImage = new BitmapImage();
            //stopImage.DecodePixelHeight = 30;
            //stopImage.UriSource = new Uri("ms-appx:///Assets/Stop.png");
            //var StopIcon = new MapIcon
            //{
            //    Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Stop.png")),
            //    CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
            //    Location = stopLocation,
            //    NormalizedAnchorPoint = new Point(0.5, 1.0),
            //    ZIndex = 100
            //};
            var stopIcon = (MapIcon) SearchMap.MapElements[0];
            stopIcon.Location = stopLocation;
            await SearchMap.TrySetViewAsync(stopLocation, 13.0);

            try
            {
                //SearchMap.MapElements.in(0,StopIcon);
                //await SearchMap.TrySetViewAsync(stopLocation, 13.0);
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
            Frame.Navigate(typeof(StopDetail), e.ClickedItem);
        }

        private void DetailButtonPressed(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StopDetail), searchVm.SelectedStop);
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
                    if (hasSearched)
                    {
                        FavoriteBtn.Visibility = Visibility.Visible;
                        DetailBtn.Visibility = Visibility.Visible;
                    }
                    SortBtn.Visibility = Visibility.Collapsed;
                    RefreshBtn.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            //Unhook event handlers and stop timer
            MainPivot.SelectionChanged -= PivotItemChanged;
            LocationHelper.LocationChanged -= UpdateLocationOnMap;
            RouteBox.SelectionChanged -= RouteSelected;
            DirBox.SelectionChanged -= DirectionSelected;
            StopBox.SelectionChanged -= StopSelected;

            if (refreshTimer != null && refreshTimer.IsEnabled)
                refreshTimer.Stop();
        }
    }
}
