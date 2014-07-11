using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace nexMuni
{
    class StopDetailModel
    {
        public static ObservableCollection<RouteData> routeList;
        public static string url { get; set; }
        public static string baseURL {get; set;}

        public static void LoadData(StopData stop)
        {
            StopDetailModel.routeList = new System.Collections.ObjectModel.ObservableCollection<RouteData>();

            int i = 0;
            StringBuilder cont = new StringBuilder();
            baseURL = "http://webservices.nextbus.com/service/publicXMLFeed?command=predictionsForMultiStops&a=sf-muni";
            
            while (i < stop.RoutesSplit.Length)
            {
                if (i == 0) routeList.Add(new RouteData(" " + stop.RoutesSplit[i]));
                else routeList.Add(new RouteData(stop.RoutesSplit[i]));

                i++; 
            }

            i = 0;
            while (i<stop.Tags.Length)
            {
                cont.Append("&stops=" + stop.Tags[i]);
                i++;
            }

            url = baseURL + cont.ToString();

            PredictionModel.SendToModel(routeList);
        }
    }
}
