using nexMuni.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace nexMuni
{
    public sealed partial class MainPage : Page
    {
        public static TextBlock noNearbyText { get; set; }
        public static TextBlock noFavsText { get; set; }
        public static TextBlock timesText { get; set; }
        public static TextBlock dirText { get; set; }
        public static TextBlock stopText { get; set; }

        public static ComboBox dirComboBox { get; set; }
        public static MapControl searchMap { get; set; }
        
        public static ListPickerFlyout routePicker { get; set; }
        public static ListPickerFlyout stopPicker { get; set; }

        public static Button routeBtn { get; set; }
        public static Button stopBtn { get; set; }
        public static Button favSearchBtn { get; set; }
        public static Button removeSearchBtn { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            pivotControl.SelectionChanged += pivotControl_SelectionChanged;
            if (!MainPageModel.IsDataLoaded)
            {
                noNearbyText = noStopsNotice;
                noFavsText = noFavsNotice;
                timesText = SearchTimes;
                dirText = DirLabel;
                stopText = StopLabel;

                searchMap = SearchMapControl;
                searchMap.Center = new Geopoint(new BasicGeoposition() { Latitude = 37.7599, Longitude = -122.437 });

                routeBtn = RouteButton;
                stopBtn = StopButton;
                favSearchBtn = AddFavSearch;
                removeSearchBtn = RemoveFavSearch;

                routePicker = RoutesFlyout;
                stopPicker = StopsFlyout;
                dirComboBox = DirBox;

                MainPageModel.LoadData();
                SearchModel.LoadData();

                nearbyListView.ItemsSource = MainPageModel.NearbyStops;
                favoritesListView.ItemsSource = MainPageModel.FavoritesStops;
            }           
        }

        void pivotControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((Pivot)sender).SelectedIndex)
            {
                case 0:
                    RefreshBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                    sortBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    break;
                case 1:
                    appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                    sortBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    RefreshBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    break;
                case 2:
                    appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                    sortBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    RefreshBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    break;
            }
        }

        private async void UpdateButtonPressed(object sender, RoutedEventArgs e)
        {
            RefreshBtn.IsEnabled = false;
            await MainPageModel.UpdateNearbyStops();
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
            await DatabaseHelper.FavoriteFromSearch(SearchModel.selectedStop);
            favSearchBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            removeSearchBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private async void RemoveSearch(object sender, RoutedEventArgs e)
        {
            await DatabaseHelper.RemoveSearch(SearchModel.selectedStop);
            removeSearchBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            favSearchBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
    }
}
