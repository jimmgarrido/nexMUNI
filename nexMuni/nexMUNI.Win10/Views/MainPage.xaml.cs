using nexMuni.Helpers;
using nexMuni.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace nexMuni.Views
{
    public sealed partial class MainPage : Page
    {
        MainViewModel mainVm;
        SearchViewModel searchVm;

        private bool alreadyLoaded;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!alreadyLoaded)
            {
                //MainPivot.SelectionChanged += PivotItemChanged;
                await DatabaseHelper.CheckDatabasesAsync();

                mainVm = new MainViewModel();
                searchVm = new SearchViewModel();
                //searchVm.UpdateLocation += LocationUpdated;

                NearbyPivot.DataContext = mainVm;
                SearchPivot.DataContext = searchVm;
                FavoritesPivot.DataContext = mainVm;

                //RoutesFlyout.ItemsPicked += RouteSelected;
                //DirBox.SelectionChanged += DirectionSelected;
                //StopsFlyout.ItemsPicked += StopSelected;

                //MapControl.SetNormalizedAnchorPoint(LocationIcon, new Point(0.5, 0.5));
                //MapControl.SetNormalizedAnchorPoint(StopIcon, new Point(0.5, 1.0));

                await searchVm.LoadRoutesAsync();
                RouteBox.IsEnabled = true;
                await mainVm.LoadAsync();

                alreadyLoaded = true;
            }
        }

        private void StopClicked(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(StopDetail), e.ClickedItem);
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

        private void DetailButtonPressed(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StopDetail), searchVm.SelectedStop);
        }

        private void GoToAbout(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(AboutPage));
        }

        private void GoToSettings(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(SettingsPage));
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
