using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using nexMuni.Helpers;
using System.Threading.Tasks;
using nexMuni.DataModels;

namespace nexMuni
{
    class StopDetailModel
    {
        public ObservableCollection<Route> Routes { get; private set; }
        public StopData SelectedStop { get; private set; }

        private string URL;

        private StopDetailModel() { }

        public StopDetailModel(StopData stop)
        {
            Routes = new ObservableCollection<Route>();
            SelectedStop = stop;
        }

        public async Task LoadTimes()
        {

#if WINDOWS_PHONE_APP
            var systemTray = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            systemTray.ProgressIndicator.Text = "Getting Arrival Times";
            systemTray.ProgressIndicator.ProgressValue = null;
#endif

            string[] splitRoutes = SelectedStop.Routes.Split(',');
            splitRoutes[0] = " " + splitRoutes[0];

            URL = WebRequests.GetMulitPredictionURL(SelectedStop.Tags);
            List<Route> routeList = await PredictionHelper.GetPredictionTimes(URL);

            foreach(Route r in routeList)
            {
                Routes.Add(r);
            }

#if WINDOWS_PHONE_APP
            systemTray.ProgressIndicator.ProgressValue = 0;
            systemTray.ProgressIndicator.Text = "nexMuni";
#endif
        }

        public async Task RefreshTimes()
        {
            Routes.Clear();
            List<Route> routeList = await PredictionHelper.GetPredictionTimes(URL);

            foreach (Route r in routeList)
            {
                Routes.Add(r);
            }
        }
    }
}
