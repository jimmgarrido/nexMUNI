using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;
using nexMuni.Helpers;
using nexMuni.ViewModels;

namespace nexMuni.Views
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
        public static Pivot mainPivot { get; set; }
        
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainPivot.SelectionChanged += pivotControl_SelectionChanged;
            if (!MainPageModel.IsDataLoaded)
            {
                noNearbyText = NoStopsNotice;
                noFavsText = NoFavsNotice;
                timesText = SearchTimes;
                dirText = DirLabel;
                stopText = StopLabel;

                mainPivot = MainPivot;
                searchMap = SearchMapControl;
                searchMap.Center = new Geopoint(new BasicGeoposition() { Latitude = 37.7599, Longitude = -122.437 });

                routeBtn = RouteButton;
                stopBtn = StopButton;
                favSearchBtn = AddFavSearch;
                removeSearchBtn = RemoveFavSearch;

                routePicker = RoutesFlyout;
                stopPicker = StopsFlyout;
                dirComboBox = DirBox;

                await DatabaseHelper.CheckDatabases();
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
                    RefreshBtn.Visibility = Visibility.Visible;
                    appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                    sortBtn.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                    sortBtn.Visibility = Visibility.Visible;
                    RefreshBtn.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    appBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                    sortBtn.Visibility = Visibility.Collapsed;
                    RefreshBtn.Visibility = Visibility.Collapsed;
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
            favSearchBtn.Visibility = Visibility.Collapsed;
            removeSearchBtn.Visibility = Visibility.Visible;
        }

        private async void RemoveSearch(object sender, RoutedEventArgs e)
        {
            await DatabaseHelper.RemoveSearch(SearchModel.selectedStop);
            removeSearchBtn.Visibility = Visibility.Collapsed;
            favSearchBtn.Visibility = Visibility.Visible;
        }
    }
}
