using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;
using nexMuni.Helpers;
using nexMuni.ViewModels;
using nexMuni.DataModels;

namespace nexMuni.Views
{
    public sealed partial class MainPage : Page
    {
        //public static TextBlock timesText { get; set; }
        //public static TextBlock dirText { get; set; }
        //public static TextBlock stopText { get; set; }

        //public static ComboBox dirComboBox { get; set; }
        //public static MapControl searchMap { get; set; }
        //public static Pivot mainPivot { get; set; }
        
        //public static ListPickerFlyout routePicker { get; set; }
        //public static ListPickerFlyout stopPicker { get; set; }

        //public static Button routeBtn { get; set; }
        //public static Button stopBtn { get; set; }
        //public static Button favSearchBtn { get; set; }
        //public static Button removeSearchBtn { get; set; }

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

                NearbyPivot.DataContext = mainVm;
                FavoritesPivot.DataContext = mainVm;
                SearchPivot.DataContext = searchVm;

                RoutesFlyout.ItemsPicked += RouteSelected;
                DirBox.SelectionChanged += DirectionSelected;
                StopsFlyout.ItemsPicked += StopSelected;

                alreadyLoaded = true;
                //timesText = SearchTimes;
                //dirText = DirLabel;
                //stopText = StopLabel;

                //mainPivot = MainPivot;
                //searchMap = SearchMapControl;
                //searchMap.Center = new Geopoint(new BasicGeoposition() { Latitude = 37.7599, Longitude = -122.437 });

                //routeBtn = RouteButton;
                //stopBtn = StopButton;
                //favSearchBtn = AddFavSearch;
                //removeSearchBtn = RemoveFavSearch;

                //routePicker = RoutesFlyout;
                //stopPicker = StopsFlyout;
                //dirComboBox = DirBox;

                //MainViewModel.LoadData();
                //SearchViewModel.LoadData();

                //NearbyListView.ItemsSource = MainViewModel.NearbyStops;
                //FavoritesListView.ItemsSource = MainViewModel.FavoritesStops;
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
                    break;
                case 1:
                    SortBtn.Visibility = Visibility.Visible;
                    RefreshBtn.Visibility = Visibility.Collapsed;
                    FavoriteBtn.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    FavoriteBtn.Visibility = Visibility.Visible;
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
            //await DatabaseHelper.FavoriteFromSearch(SearchViewModel.selectedStop);
            //favSearchBtn.Visibility = Visibility.Collapsed;
            //removeSearchBtn.Visibility = Visibility.Visible;
        }

        private async void RemoveSearch(object sender, RoutedEventArgs e)
        {
            //await DatabaseHelper.RemoveSearch(SearchViewModel.selectedStop);
            //removeSearchBtn.Visibility = Visibility.Collapsed;
            //favSearchBtn.Visibility = Visibility.Visible;
        }

        private async void RouteSelected(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            RoutesFlyout.SelectedIndex = sender.SelectedIndex;
            await searchVm.RouteSelected(sender.SelectedItem.ToString());

            DirBox.SelectedIndex = 0;
            StopsFlyout.SelectedIndex = -1;

            DirLabel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            DirBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        public async void DirectionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                StopsFlyout.SelectedIndex = -1;
                await searchVm.LoadStops(((ComboBox)sender).SelectedItem.ToString());

                StopLabel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                StopButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        public async void StopSelected(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            if (sender.SelectedIndex != -1)
            {
                await searchVm.StopSelected((Stop)sender.SelectedItem);
                SearchTimes.Visibility = Visibility.Visible;
            }
        }
    }
}
