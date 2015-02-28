using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace nexMuni
{
    class StopDetailModel
    {
        public static ObservableCollection<Route> routeList;
        public static string baseURL {get; set;}
        public static StopData selectedStop { get; set; }

        public static async void LoadData(StopData stop)
        {
#if WINDOWS_PHONE_APP
            var systemTray = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            systemTray.ProgressIndicator.Text = "Getting Arrival Times";
            systemTray.ProgressIndicator.ProgressValue = null;
#endif

            selectedStop = stop;

            baseURL = "http://webservices.nextbus.com/service/publicXMLFeed?command=predictionsForMultiStops&a=sf-muni";
            StringBuilder cont = new StringBuilder();
            
            int i = 0;
            string[] splitTags = stop.Tags.Split(',');

            string[] splitRoutes = stop.Routes.Split(',');
            splitRoutes[0] = " " + splitRoutes[0];

            while (i < splitTags.Length)
            {
                cont.Append("&stops=" + splitTags[i]);
                i++;
            }

            string url = baseURL + cont.ToString();

            PredictionModel.GetTimes(await PredictionModel.GetXML(url));

#if WINDOWS_PHONE_APP
            systemTray.ProgressIndicator.ProgressValue = 0;
            systemTray.ProgressIndicator.Text = "nexMuni";
#endif
        }
    }
}
