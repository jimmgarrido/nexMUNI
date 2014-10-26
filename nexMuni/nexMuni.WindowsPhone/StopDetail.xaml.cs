using nexMuni.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace nexMuni
{
    public sealed partial class StopDetail : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public static TextBlock noTimeText { get; set; }

        public StopDetail()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            StopDetailModel.selectedStop = e.NavigationParameter as StopData;

            StopHeader.Text = StopDetailModel.selectedStop.Name;
            removeBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            favBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            noTimeText = new TextBlock();
            noTimeText = noTimes;
            noTimes.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (StopDetailModel.routeList == null) StopDetailModel.routeList = new System.Collections.ObjectModel.ObservableCollection<RouteData>();
            else if (StopDetailModel.routeList != null) StopDetailModel.routeList.Clear();
            RouteInfoList.ItemsSource = StopDetailModel.routeList;

            //Check if the stop is in user's favorites list
            if (MainPageModel.favoritesStops.Any(x => x.Name == StopDetailModel.selectedStop.Name))
            {
                foreach (StopData s in MainPageModel.favoritesStops)
                {
                    if (s.Name == StopDetailModel.selectedStop.Name) StopDetailModel.selectedStop.FavID = s.FavID;
                }
                removeBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                favBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else favBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;

            //noTimes.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            StopDetailModel.LoadData(StopDetailModel.selectedStop);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void RefreshTimes(object sender, RoutedEventArgs e)
        {
            PredictionModel.UpdateTimes();
        }

        private void FavoriteStop(object sender, RoutedEventArgs e)
        {
            DatabaseHelper.AddFavorite(StopDetailModel.selectedStop);
            favBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            removeBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void RemoveStop(object sender, RoutedEventArgs e)
        {
            DatabaseHelper.RemoveFavorite(StopDetailModel.selectedStop);
            removeBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            favBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
            StopDetailModel.routeList.Clear();
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void ShowRouteMap(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(RouteMap), e.ClickedItem);
        }
    }
}
