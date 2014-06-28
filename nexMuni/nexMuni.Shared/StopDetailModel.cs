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

        public static void LoadData(StopData stop)
        {
            int i = 0;
            int len = stop.RoutesSplit.Length;
            while (i < len)
            {
                routeList.Add(new RouteData(stop.RoutesSplit[i]));
                i++;

            }
        }
    }
}
