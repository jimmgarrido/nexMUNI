using System;
using System.Linq;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using nexMuni.Common;
using Windows.Devices.Geolocation;
using Windows.UI.StartScreen;

namespace nexMuni.Views
{
    public sealed partial class StopDetail : Page
    {
        private NavigationHelper navigationHelper;
        private bool alreadyLoaded;

        public StopDetail()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            
           
        }

        private async void RefreshTimes(object sender, RoutedEventArgs e)
        {
          
        }

        private async void FavoriteBtnPressed(object sender, RoutedEventArgs e)
        {
            
        }

        private async void UnfavoriteBtnPressed(object sender, RoutedEventArgs e)
        {
            
        }

        private void GoToRouteMap(object sender, ItemClickEventArgs e)
        {

        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);

            if (e.Content.GetType() != typeof(RouteMapPage))
            {
                alreadyLoaded = false;

                if (FavButton.Label == "favorite")
                    FavButton.Click -= FavoriteBtnPressed;
                else
                    FavButton.Click -= UnfavoriteBtnPressed;
            }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) { }

        #endregion

    }
}
