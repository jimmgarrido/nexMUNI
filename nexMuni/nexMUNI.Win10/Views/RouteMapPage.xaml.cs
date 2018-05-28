using System;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using nexMuni.Common;
using System.Threading.Tasks;
using System.Linq;
using Windows.Storage.Streams;
using Windows.UI;
using System.IO;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace nexMuni.Views
{
    public sealed partial class RouteMapPage : Page
    {
        private NavigationHelper navigationHelper;
        private bool alreadyLoaded;
        private DispatcherTimer refreshTimer;
        private int vehicleCounter = 0;

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public RouteMapPage()
        {
            this.InitializeComponent(); 

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            
        }

        private async Task ShowRoutePath()
        {
            
        }

        private async Task AddVehicleLocations()
        {
            
        }

        private async void TimerDue(object sender, object e)
        {
            await AddVehicleLocations();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
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
            refreshTimer.Stop();
            refreshTimer.Tick -= TimerDue;
        }

        #endregion
    }
}
