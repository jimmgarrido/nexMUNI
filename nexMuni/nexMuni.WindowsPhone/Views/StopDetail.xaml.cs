using System.Linq;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using nexMuni.Common;
using nexMuni.Helpers;
using nexMuni.ViewModels;
using nexMuni.DataModels;

namespace nexMuni.Views
{
    public sealed partial class StopDetail : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private bool alreadyLoaded;

        public StopDetailViewModel detailVm;

        public StopDetail()
        {
            this.InitializeComponent();

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
                    FavButton.Icon = new SymbolIcon(Symbol.Remove);
                }
                else
                {
                    FavButton.Click += FavoriteBtnPressed;
                    FavButton.Label = "favorite";
                    FavButton.Icon = new SymbolIcon(Symbol.Favorite);
                }

                alreadyLoaded = true;
            }

            await detailVm.LoadTimes();
        }

        private async void RefreshTimes(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            var systemTray = StatusBar.GetForCurrentView();
            systemTray.ProgressIndicator.Text = "Refreshing Times";
            systemTray.ProgressIndicator.ProgressValue = null;
#endif
            RefreshBtn.IsEnabled = false;
            await detailVm.RefreshTimes();
            RefreshBtn.IsEnabled = true;

#if WINDOWS_PHONE_APP
            systemTray.ProgressIndicator.ProgressValue = 0;
            systemTray.ProgressIndicator.Text = "nexMuni";
#endif
        }

        private async void FavoriteBtnPressed(object sender, RoutedEventArgs e)
        {
            await detailVm.AddFavoriteAsync();
            FavButton.Click -= FavoriteBtnPressed;
            FavButton.Click += UnfavoriteBtnPressed;
            FavButton.Icon = new SymbolIcon(Symbol.Remove);
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
            Frame.Navigate(typeof(RouteMapPage), e.ClickedItem);
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
            detailVm.StopTimer();
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) { }

        #endregion

    }
}
