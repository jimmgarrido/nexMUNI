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
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!MainPageModel.IsDataLoaded)
            {
                MainPageModel.LoadData();
                nearbyListView.ItemsSource = MainPageModel.nearbyStops;
                favoritesListView.ItemsSource = MainPageModel.favoritesStops;
            }
        }

        private void UpdateButton(object sender, RoutedEventArgs e)
        {
            MainPageModel.nearbyStops.Clear();
            LocationHelper.UpdateNearbyList();
        }

        private void StopClicked(object sender, ItemClickEventArgs e)
        {
            StopData selected = e.ClickedItem as StopData;
            this.Frame.Navigate(typeof(StopDetail), e.ClickedItem);
        }

        private void GoToAbout(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AboutPage));
        }
    }
}
