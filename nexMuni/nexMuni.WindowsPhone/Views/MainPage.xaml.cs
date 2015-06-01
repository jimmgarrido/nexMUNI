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
        }

        private async void DirectionSelected(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex != -1)
            {
                StopsFlyout.SelectedIndex = -1;
                await searchVm.LoadStopsAsync(((ComboBox)sender).SelectedItem.ToString());

                StopLabel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                StopButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private async void StopSelected(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            if (sender.SelectedIndex != -1)
            {
                await searchVm.StopSelectedAsync((Stop)sender.SelectedItem);
                SearchTimes.Visibility = Visibility.Visible;
                FavoriteBtn.IsEnabled = true;
            }
        }
    }
}
