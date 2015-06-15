using System;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;
using nexMuni.Helpers;
using nexMuni.ViewModels;
using nexMuni.DataModels;
using Windows.Foundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace nexMuni.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel mainVm;
        public SearchViewModel searchVm;

        private bool alreadyLoaded;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!alreadyLoaded)
            {
                MainPivot.SelectionChanged += pivotControl_SelectionChanged;
                await DatabaseHelper.CheckDatabasesAsync();

                mainVm = new MainViewModel();
                searchVm = new SearchViewModel();
                searchVm.UpdateLocation += UpdateLocationIcon;

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
        }

        void pivotControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            this.Frame.Navigate(typeof(AboutPage));
        }

        private void SortFavorites(object sender, RoutedEventArgs e)
        {
            LocationHelper.SortFavorites();
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
            StopsFlyout.SelectedIndex = -1;

            DirLabel.Visibility = Visibility.Visible;
            DirBox.Visibility = Visibility.Visible;
            StopIcon.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            FavoriteBtn.IsEnabled = false;
            DetailBtn.IsEnabled = false;

            await ShowRoutePath();
        }

        private void DirectionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                StopsFlyout.SelectedIndex = -1;
                searchVm.LoadStops(((ComboBox)sender).SelectedItem.ToString());

                StopLabel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                StopButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                StopIcon.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            FavoriteBtn.IsEnabled = false;
            DetailBtn.IsEnabled = false;
        }

        private async void StopSelected(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            if (sender.SelectedIndex != -1)
            {
                await searchVm.StopSelectedAsync((Stop)sender.SelectedItem);
                SearchTimes.Visibility = Visibility.Visible;

                if (searchVm.IsFavorite())
                {
                    FavoriteBtn.Click += UnfavoriteSearch;
                    FavoriteBtn.Label = "unfavorite";
                    FavoriteBtn.Icon = new SymbolIcon(Symbol.Remove);
                }
                else
                {
                    FavoriteBtn.Click += FavoriteSearch;
                    FavoriteBtn.Label = "favorite";
                    FavoriteBtn.Icon = new SymbolIcon(Symbol.Favorite);
                }
                FavoriteBtn.IsEnabled = true;
                DetailBtn.IsEnabled = true;

                await ShowStopLocation();
            } 
        }

        private void DetailButtonPressed(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StopDetail), searchVm.SelectedStop);
        }

        private void UpdateLocationIcon()
        {
            MapControl.SetLocation(LocationIcon, LocationHelper.Location.Coordinate.Point);
        }

        private async Task ShowRoutePath()
        {
            List<MapPolyline> routePath = await MapHelper.LoadDoc(searchVm.SelectedRoute);
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

        private async Task ShowStopLocation()
        {
            var stopLocation =  new Geopoint(new BasicGeoposition() { Latitude = searchVm.SelectedStop.Latitude, Longitude = searchVm.SelectedStop.Longitude });   
            MapControl.SetLocation(StopIcon, stopLocation);
            StopIcon.Visibility = Windows.UI.Xaml.Visibility.Visible;
            await SearchMap.TrySetViewAsync(stopLocation, 13.0);
        }
    }
}
