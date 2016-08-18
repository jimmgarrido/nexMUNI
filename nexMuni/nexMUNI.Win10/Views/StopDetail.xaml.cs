using System;
using System.Linq;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using nexMuni.Common;
using nexMuni.Helpers;
using nexMuni.ViewModels;
using nexMuni.DataModels;
using Windows.Devices.Geolocation;
using Windows.UI.StartScreen;

namespace nexMuni.Views
{
    public sealed partial class StopDetail : Page
    {
        private NavigationHelper navigationHelper;
        private bool alreadyLoaded;

        private StopDetailViewModel detailVm;

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
            if (!alreadyLoaded)
            {
                detailVm = new StopDetailViewModel(e.NavigationParameter as Stop);
                DataContext = detailVm;

                //Check if the stop is in user's favorites list
                if(detailVm.IsFavorite())
                {
                    FavButton.Click += UnfavoriteBtnPressed;
                    FavButton.Label = "unfavorite";
                    FavButton.Icon = new SymbolIcon(Symbol.UnFavorite);
                }
                else
                {
                    FavButton.Click += FavoriteBtnPressed;
                    FavButton.Label = "favorite";
                    FavButton.Icon = new SymbolIcon(Symbol.Favorite);
                }

                if(!SecondaryTile.Exists(detailVm.tileId))
                {
                    PinButton.Click += detailVm.PinTile;
                    PinButton.Label = "pin";
                    PinButton.Icon = new SymbolIcon(Symbol.Pin);
                }
                else
                {
                    
                    PinButton.Label = "unpin";
                    PinButton.Icon = new SymbolIcon(Symbol.UnPin);
                }

                if (!await detailVm.LoadTimes()) //TODO: Make Timer accessible so I do not need to load all times just to restart it.
                    //TODO: Times do not automatically refresh after navigating back from map page
                {
                    Frame.GoBack();
                }

                alreadyLoaded = true;
            }

            await UIHelper.ShowStatusBar("Getting Arrival Times");

            //if (!detailVm.Alerts.Any()) DetailPivot.Items.RemoveAt(1);

            LoadingRing.IsActive = false;
            await UIHelper.HideStatusBar();
        }

        private async void RefreshTimes(object sender, RoutedEventArgs e)
        {
            RefreshBtn.IsEnabled = false;
            await detailVm.RefreshTimes();
            RefreshBtn.IsEnabled = true;
        }

        private async void FavoriteBtnPressed(object sender, RoutedEventArgs e)
        {
            await detailVm.AddFavoriteAsync();
            FavButton.Click -= FavoriteBtnPressed;
            FavButton.Click += UnfavoriteBtnPressed;
            FavButton.Icon = new SymbolIcon(Symbol.UnFavorite);
            FavButton.Label = "unfavorite";
        }

        private async void UnfavoriteBtnPressed(object sender, RoutedEventArgs e)
        {
            await detailVm.RemoveFavoriteAsync();
            FavButton.Click -= UnfavoriteBtnPressed;
            FavButton.Click += FavoriteBtnPressed;
            FavButton.Icon = new SymbolIcon(Symbol.Favorite);
            FavButton.Label = "favorite";
        }

        private void GoToRouteMap(object sender, ItemClickEventArgs e)
        {
            var route = (Route)e.ClickedItem;
            route.stopLocation = new Geopoint(new BasicGeoposition { Latitude = detailVm.SelectedStop.Latitude, Longitude = detailVm.SelectedStop.Longitude });

            Frame.Navigate(typeof(RouteMapPage), route);
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
            detailVm.StopTimer();

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
