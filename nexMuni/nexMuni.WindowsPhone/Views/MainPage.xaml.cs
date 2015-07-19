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

namespace nexMuni.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel mainVm;
        public SearchViewModel searchVm;

        private bool alreadyLoaded;
        private int vehicleCounter;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (alreadyLoaded) return;
            MainPivot.SelectionChanged += PivotItemChanged;
            MainPivot.SelectedIndex = SettingsHelper.GetLaunchPivotSetting();
            await DatabaseHelper.CheckDatabasesAsync();

            mainVm = new MainViewModel();
            searchVm = new SearchViewModel();
            searchVm.UpdateLocation += LocationUpdated;

            NearbyPivot.DataContext = mainVm;
            FavoritesPivot.DataContext = mainVm;
            SearchPivot.DataContext = searchVm;

            RoutesFlyout.ItemsPicked += RouteSelected;
            DirBox.SelectionChanged += DirectionSelected;
            StopsFlyout.ItemsPicked += StopSelected;

            MapControl.SetNormalizedAnchorPoint(LocationIcon, new Point(0.5, 0.5));
            MapControl.SetNormalizedAnchorPoint(StopIcon, new Point(0.5, 1.0));

            alreadyLoaded = true;
        }

        private async void UpdateButtonPressed(object sender, RoutedEventArgs e)
        {
            RefreshBtn.IsEnabled = false;
            await mainVm.UpdateNearbyStops();
            RefreshBtn.IsEnabled = true;
        }

        private void StopClicked(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(StopDetail), e.ClickedItem);
        }

        private void GoToAbout(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

        private void GoToSettings(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
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

        private async void RouteSelected(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            RoutesFlyout.SelectedIndex = sender.SelectedIndex;
            await searchVm.LoadDirectionsAsync(sender.SelectedItem.ToString());

            DirBox.SelectedIndex = 0;
            //StopsFlyout.SelectedIndex = -1;

            DirLabel.Visibility = Visibility.Visible;
            DirBox.Visibility = Visibility.Visible;
            StopIcon.Visibility = Visibility.Collapsed;

            FavoriteBtn.IsEnabled = false;
            DetailBtn.IsEnabled = false;

            while (vehicleCounter > 0)
            {
                SearchMap.Children.RemoveAt(SearchMap.Children.Count - 1);
                vehicleCounter--;
            }

            await ShowRoutePath();
            await ShowVehicleLocations();
        }

        private void DirectionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                StopsFlyout.SelectedIndex = -1;
                searchVm.LoadStops(((ComboBox)sender).SelectedItem.ToString());

                StopLabel.Visibility = Visibility.Visible;
                StopButton.Visibility = Visibility.Visible;
                StopIcon.Visibility = Visibility.Collapsed;
            }

            FavoriteBtn.IsEnabled = false;
            DetailBtn.IsEnabled = false;
        }

        private async void StopSelected(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            if (sender.SelectedIndex == -1) return;
            await searchVm.StopSelectedAsync((Stop)sender.SelectedItem);
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

        private void DetailButtonPressed(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StopDetail), searchVm.SelectedStop);
        }

        private void LocationUpdated()
        {
            RefreshBtn.IsEnabled = true;
            MapControl.SetNormalizedAnchorPoint(LocationIcon, new Point(0.5, 0.5));
            MapControl.SetLocation(LocationIcon, LocationHelper.Location.Coordinate.Point);
            mainVm.FavoritesDistances();
            SortBtn.IsEnabled = true;
        }

        private async Task ShowRoutePath()
        {
            var xmlDoc = await WebHelper.GetRoutePathAsync(searchVm.SelectedRoute);
            var routePath = await MapHelper.ParseRoutePath(xmlDoc);
    
            SearchMap.MapElements.Clear();

            if (routePath.Any())
            {
                foreach (MapPolyline line in routePath)
                {
                    SearchMap.MapElements.Add(line);
                }
            }

            await SearchMap.TrySetViewAsync(searchVm.MapCenter, 11.40);
        }
        private async Task ShowVehicleLocations()
        {
            var xmlDoc = await WebHelper.GetBusLocationsAsync(searchVm.SelectedRoute);
            var vehicleLocations = MapHelper.ParseBusLocations(xmlDoc);

            var inboundBus = new BitmapImage(new Uri("ms-appx:///Assets/Inbound.png"));
            var outboundBus = new BitmapImage(new Uri("ms-appx:///Assets/Outbound.png"));

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
            await SearchMap.TrySetViewAsync(stopLocation, 13.0);
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
