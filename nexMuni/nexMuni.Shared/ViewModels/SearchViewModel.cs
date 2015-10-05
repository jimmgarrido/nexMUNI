using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.UI.ViewManagement;
using Windows.Web.Http;
using nexMuni.DataModels;
using nexMuni.Helpers;

namespace nexMuni.ViewModels
{
    public delegate void ChangedEventHandler();

    public class SearchViewModel : INotifyPropertyChanged
    {
        private string _searchTimes;
        private string _selectedRoute;
        private List<string> _routesList;
        private List<string> _directionsList;
        private List<Stop> _stopsList;
        private Stop _selectedStop;
        private Geopoint _mapCenter;

        public string SearchTimes
        {
            get
            {
                return _searchTimes;
            }
            set
            {
                _searchTimes = value;
                NotifyPropertyChanged("SearchTimes");
            }
        }
        public string SelectedRoute
        {
            get
            {
                return _selectedRoute;
            }
            set
            {
                _selectedRoute = value;
                NotifyPropertyChanged("SelectedRoute");
            }
        }
        public Stop SelectedStop
        {
            get
            {
                return _selectedStop;
            }
            set
            {
                _selectedStop = value;
                NotifyPropertyChanged("SelectedStop");
            }
        }

        public List<string> RoutesList
        {
            get
            {
                return _routesList;
            }
            set
            {
                _routesList = value;
                NotifyPropertyChanged("RoutesList");
            }
        }
        public List<string> DirectionsList
        {
            get
            {
                return _directionsList;
            }
            set
            {
                _directionsList = value;
                NotifyPropertyChanged("DirectionsList");
            }
        }
        public List<Stop> StopsList
        {
            get
            {
                return _stopsList;
            }
            set
            {
                _stopsList = value;
                NotifyPropertyChanged("StopsList");
            }
        }
        public Geopoint MapCenter
        {
            get
            {
                return _mapCenter;
            }
            set
            {
                _mapCenter = value;
                NotifyPropertyChanged("MapCenter");
            }
        }

        private Task initialize;
        private Stop foundStop;
        private List<Stop> allStopsList;
        private List<string> outboundStops = new List<string>();
        private List<string> inboundStops = new List<string>();

        public SearchViewModel()
        {
            DatabaseHelper.FavoritesChanged += SyncFavoriteIds;
            MapCenter = new Geopoint(new BasicGeoposition() { Latitude = 37.7480, Longitude = -122.437 });

            allStopsList = new List<Stop>();
            outboundStops = new List<string>();
            inboundStops = new List<string>();

        }

        public async Task LoadRoutesAsync()
        {
            RoutesList = await DatabaseHelper.QueryForRoutes();
        }

        public async Task LoadDirectionsAsync(string route)
        {
            SelectedRoute = route;
            SelectedStop = null;

            if (route.Equals("Powell/Mason Cable Car")) route = "59";
            else if (route.Equals("Powell/Hyde Cable Car")) route = "60";
            else if (route.Equals("California Cable Car")) route = "61";
            else
            {
                route = route.Substring(0, route.IndexOf('-'));
            }

            var xmlDoc = await WebHelper.GetRouteDirections(route);
            DirectionsList = await Task.Run(() => ParseHelper.ParseDirections(xmlDoc));
            allStopsList = await Task.Run(() => ParseHelper.ParseStops(xmlDoc));
            await Task.Run(() => ParseHelper.ParseStopTags(xmlDoc, inboundStops, outboundStops));
            //await SortStops(xmlDoc);
        }

        public async Task LoadStops(string direction)
        {
            var foundStops = new List<Stop>();

            SelectedStop = null;
            SearchTimes = "";

            await Task.Run(() =>
            {
                if (direction.Contains("Inbound"))
                {
                    foreach (string tag in inboundStops)
                    {
                        foundStops.Add(allStopsList.Find(z => z.StopTags == tag));
                    }
                }
                else if (direction.Contains("Outbound"))
                {
                    foreach (string tag in outboundStops)
                    {
                        foundStops.Add(allStopsList.Find(z => z.StopTags == tag));
                    }
                }
            });

            StopsList = foundStops;
        }

        public async Task StopSelectedAsync(Stop stop)
        {
            SelectedStop = stop;
            string title = SelectedStop.StopName;

            if (title.Contains("Inbound"))
            {
                SelectedStop.StopName = title.Replace(" Inbound", "");
            }
            if (title.Contains("Outbound"))
            {
                SelectedStop.StopName = title.Replace(" Outbound", "");
            }

            string[] temp = SelectedStop.StopName.Split('&');
            string reversed;
            if (temp.Count() > 1)
            {
                reversed = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));
            }
            else reversed = "";

            var xmlDoc = await WebHelper.GetSearchPredictionsAsync(SelectedStop, SelectedRoute);
            if (xmlDoc != null)
            {
                //Get bus predictions for stop
                SearchTimes = await Task.Run(() => ParseHelper.ParseSearchTimesAsync(xmlDoc));

                Stop tempStop = await GetStopAsync();
                if (tempStop != null) SelectedStop = tempStop;
            }
        }

        public async Task FavoriteSearchAsync()
        {
            await DatabaseHelper.FavoriteSearchAsync(SelectedStop);
        }

        public async Task UnfavoriteSearchAsync()
        {
            await DatabaseHelper.RemoveFavoriteAsync(SelectedStop);
        }

        public bool IsFavorite()
        {
            return DatabaseHelper.FavoritesList.Any(f => f.Name == SelectedStop.StopName);
        }

        private async Task<Stop> GetStopAsync()
        {
            List<Stop> stops = await DatabaseHelper.QueryForStop(SelectedStop.StopName);

            if (stops.Any())
                return stops.ElementAt(0);
            else return null;
        }

        //private async Task SortStops(XDocument doc)
        //{
        //    var tagElements = await Task.Run(() => ParseHelper.ParseStopTags(doc));
        //    foreach (string direction in DirectionsList)
        //    {
        //        IEnumerable<XElement> tagElements;
        //        if (el.Attribute("name").Value == "Inbound")
        //        {
        //            //Get all stop elements under direction element
        //            tagElements =
        //                from x in el.Elements("stop")
        //                select x;

        //            if (inboundStops.Count != 0) inboundStops.Clear();
        //            //Add tags for direction to a collection
        //            foreach (XElement y in tagElements)
        //            {
        //                inboundStops.Add(y.Attribute("tag").Value);
        //            }
        //        }
        //        else if (el.Attribute("name").Value == "Outbound")
        //        {
        //            //Get all stop elements under direction element
        //            tagElements =
        //                from x in el.Elements("stop")
        //                select x;

        //            if (outboundStops.Count != 0) outboundStops.Clear();
        //            //Add tags for direction to a collection
        //            foreach (XElement y in tagElements)
        //            {
        //                outboundStops.Add(y.Attribute("tag").Value);
        //            }
        //        }
        //    }
        //    return directions;
        //}

        private void SyncFavoriteIds()
        {
            if (SelectedStop == null) return;
            FavoriteData tempStop = DatabaseHelper.FavoritesList.ToList().Find(s => s.Name == SelectedStop.StopName);
            if (tempStop == null) return;
            SelectedStop.favId = tempStop.Id;
        }

        #region INotify Methods
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
