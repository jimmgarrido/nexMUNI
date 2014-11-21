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
                noNearbyText = new TextBlock();
                noFavsText = new TextBlock();
                timesText = new TextBlock();
                dirText = new TextBlock();
                stopText = new TextBlock();

                searchMap = new MapControl();
                searchMap = searchMapControl;
                searchMap.Center = new Geopoint(new BasicGeoposition() { Latitude = 37.7599, Longitude = -122.437 });
                
                routeBtn = new Button();
                stopBtn = new Button();
                favSearchBtn = new Button();
                removeSearchBtn = new Button();

                routePicker = new ListPickerFlyout();
                stopPicker = new ListPickerFlyout();
                dirComboBox = new ComboBox();

                noNearbyText = noStopsNotice;
                noFavsText = noFavsNotice;
                timesText = searchTimes;
                dirText = dirLabel;
                stopText = stopLabel;

                routeBtn = routeButton;
                stopBtn = stopButton;
                favSearchBtn = AddFavSearch;
                removeSearchBtn = RemoveFavSearch;

                dirComboBox = dirBox;
                routePicker = RoutesFlyout;
                stopPicker = StopsFlyout;

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
                    appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
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
                    break;
            }
        }

        private async void UpdateButton(object sender, RoutedEventArgs e)
        {
            MainPageModel.nearbyStops.Clear();
            noNearbyText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            await LocationHelper.UpdateNearbyList();
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
