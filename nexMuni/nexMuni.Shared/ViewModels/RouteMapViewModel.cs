using nexMuni.DataModels;
using nexMuni.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml.Controls.Maps;

namespace nexMuni.ViewModels
{
    public class RouteMapViewModel
    {
        public Route SelectedRoute { get; set; }
        //public List<MapPolyline> routePath { get; set; }

        public RouteMapViewModel(Route route)
        {
            SelectedRoute = route;
        }

        public async Task<List<MapPolyline>> GetRoutePath()
        {
            var xmlDoc = await WebHelper.GetRoutePathAsync(SelectedRoute.RouteNumber);
            return await MapHelper.ParseRoutePath(xmlDoc);
        }

        public async Task<List<Bus>> GetBusLocations()
        {
            var xmlDoc = await WebHelper.GetBusLocationsAsync(SelectedRoute.RouteNumber);
            return await MapHelper.ParseBusLocations(xmlDoc);
        }
    }
}
