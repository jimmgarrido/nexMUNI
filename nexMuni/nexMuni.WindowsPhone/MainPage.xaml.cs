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
        public static TextBlock nearbyText { get; set; }
        public static TextBlock favText { get; set; }
        public static ComboBox dirComboBox { get; set; }
        public static MapControl searchMap { get; set; }
        public static TextBlock searchText { get; set; }
        public static ListPickerFlyout routePicker { get; set; }
        public static ListPickerFlyout stopPicker { get; set; }
        public static TextBlock dirText { get; set; }
        public static TextBlock stopText { get; set; }
        public static Button stopBtn { get; set; }
        public static Button favSearchBtn { get; set; }

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
                nearbyText = new TextBlock();
                favText = new TextBlock();
                searchMap = new MapControl();

                nearbyText = noStops;
                favText = noFav;
                searchMap = searchMapControl;
                searchMap.Center = new Geopoint(new BasicGeoposition() { Latitude = 37.7599, Longitude = -122.437 });

                dirComboBox = new ComboBox();
                routePicker = new ListPickerFlyout();
                stopPicker = new ListPickerFlyout();
                dirText = new TextBlock();
                stopText = new TextBlock();
                searchText = new TextBlock();
                stopBtn = new Button();
                favSearchBtn = new Button();

                dirComboBox = dirBox;
                routePicker = RoutesFlyout;
                stopPicker = StopsFlyout;
                searchText = searchTimes;
                dirText = dirLabel;
                stopText = stopLabel;
                stopBtn = stopButton;
                favSearchBtn = AddFavSearch;

                SearchModel.LoadRoutes();
                MainPageModel.LoadData();

                nearbyListView.ItemsSource = MainPageModel.nearbyStops;
                favoritesListView.ItemsSource = MainPageModel.favoritesStops;
            }           
        }

        void pivotControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((Pivot)sender).SelectedIndex)
            {
                case 0:
                    refreshBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                    sortBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    break;
                case 1:
                    appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                    sortBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    refreshBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    break;
                case 2:
                    appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                    sortBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    refreshBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    if (!SearchModel.IsDataLoaded)
                    {
                        dirBox.ItemsSource = SearchModel.DirectionCollection;
                        stopPicker.ItemsSource = SearchModel.StopCollection;

                        routePicker.ItemsPicked += SearchModel.RouteSelected;
                        dirBox.SelectionChanged += SearchModel.DirSelected;
                        stopPicker.ItemsPicked += SearchModel.StopSelected;

                        SearchModel.IsDataLoaded = true;
                    }
                    break;
            }
        }

        private void UpdateButton(object sender, RoutedEventArgs e)
        {
            MainPageModel.nearbyStops.Clear();
            nearbyText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            LocationHelper.UpdateNearbyList();
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

        private void FavoriteSearch(object sender, RoutedEventArgs e)
        {
            DatabaseHelper.FavoriteFromSearch(stopPicker.SelectedItem);
            favSearchBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            
        }

        private void RemoveSearch(object sender, RoutedEventArgs e)
        {
            DatabaseHelper.FavoriteFromSearch(stopPicker.SelectedItem);
            favSearchBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
