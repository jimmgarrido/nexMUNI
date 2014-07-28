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
        public static ObservableCollection<RouteData> routeList;
        public static string url { get; set; }
        public static string baseURL {get; set;}

        public static void LoadData(StopData stop)
        {            
            baseURL = "http://webservices.nextbus.com/service/publicXMLFeed?command=predictionsForMultiStops&a=sf-muni";
            StringBuilder cont = new StringBuilder();
            
            int i = 0;
            if(stop.TagsSplit == null)
            {
                stop.TagsSplit = stop.Tags.Split(',');
            }

            if (stop.RoutesSplit == null)
            {
                stop.RoutesSplit = stop.Routes.Split(',');
                stop.RoutesSplit[0] = " " + stop.RoutesSplit[0];
            }

            while (i < stop.TagsSplit.Length)
            {
                cont.Append("&stops=" + stop.TagsSplit[i]);
                i++;
            }

            url = baseURL + cont.ToString();

            PredictionModel.GetXML(url, stop); 
        }
    }
}
