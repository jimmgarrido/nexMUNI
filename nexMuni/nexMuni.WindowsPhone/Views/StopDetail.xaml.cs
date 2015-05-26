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

        private StopDetailViewModel detailModel;

        public StopDetail()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            detailModel = new StopDetailViewModel(e.NavigationParameter as Stop);

            StopHeader.Text = detailModel.SelectedStop.StopName;

            RouteInfoList.ItemsSource = detailModel.Routes;

            //Check if the stop is in user's favorites list
            //if (MainViewModel.FavoritesStops.Any(x => x.Name == detailModel.SelectedStop.Name))
            //{
            //    foreach (StopData s in MainViewModel.FavoritesStops)
            //    {
            //        if (s.Name == detailModel.SelectedStop.Name) detailModel.SelectedStop.FavID = s.FavID;
            //    }
            //    favBtn.Click += RemoveStop;
            //    favBtn.Label = "unfavorite";
            //    favBtn.Icon = new SymbolIcon(Symbol.Remove);
            //}
            //else favBtn.Click += FavoriteStop;

            await detailModel.LoadTimes();

            noTimesBlock.Visibility = detailModel.Routes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private async void RefreshTimes(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            var systemTray = StatusBar.GetForCurrentView();
            systemTray.ProgressIndicator.Text = "Refreshing Times";
            systemTray.ProgressIndicator.ProgressValue = null;
#endif
            RefreshBtn.IsEnabled = false;
            await detailModel.RefreshTimes();
            RefreshBtn.IsEnabled = true;
            noTimesBlock.Visibility = detailModel.Routes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

#if WINDOWS_PHONE_APP
            systemTray.ProgressIndicator.ProgressValue = 0;
            systemTray.ProgressIndicator.Text = "nexMuni";
#endif
        }

        private async void FavoriteStop(object sender, RoutedEventArgs e)
        {
            //await DatabaseHelper.AddFavorite(detailModel.SelectedStop);
            favBtn.Click -= FavoriteStop;
            favBtn.Click += RemoveStop;
            favBtn.Icon = new SymbolIcon(Symbol.Remove);
            favBtn.Label = "unfavorite";
        }

        private async void RemoveStop(object sender, RoutedEventArgs e)
        {
            //await DatabaseHelper.RemoveFavorite(detailModel.SelectedStop);
            favBtn.Click -= RemoveStop;
            favBtn.Click += FavoriteStop;
            favBtn.Icon = new SymbolIcon(Symbol.Favorite);
            favBtn.Label = "favorite";
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
        }

        #endregion

        private void ShowRouteMap(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(RouteMap), e.ClickedItem);
        }
    }
}
