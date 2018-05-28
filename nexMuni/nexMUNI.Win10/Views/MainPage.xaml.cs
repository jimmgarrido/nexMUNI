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

namespace nexMuni.Views
{
    public sealed partial class MainPage : Page
    {
        private bool hasSearched;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await SearchVM.InitializeAsync();
        }

        private async void RouteSelected(object sender, SelectionChangedEventArgs args)
        {
        }

        private async void DirectionSelected(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private async void StopSelected(object sender, SelectionChangedEventArgs args)
        {
            
        }

        private async Task ShowRoutePath()
        {
            
        }

        private async Task ShowVehicleLocations()
        {
            
        }

        private async Task ShowStopLocation()
        {
            
        }

        private void UpdateLocationOnMap()
        {
            
        }

        private async void UpdateButtonPressed(object sender, RoutedEventArgs e)
        {
            
        }

        private void SortFavorites(object sender, RoutedEventArgs e)
        {
        }

        private async void FavoriteSearch(object sender, RoutedEventArgs e)
        {
            
        }

        private async void UnfavoriteSearch(object sender, RoutedEventArgs e)
        {
            
        }

        private void StopClicked(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(StopDetail), e.ClickedItem);
        }

        private void DetailButtonPressed(object sender, RoutedEventArgs e)
        {
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
        }
    }
}
