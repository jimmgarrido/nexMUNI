using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NexMuni.Helpers;
using NextBus.Models;
using Windows.Devices.Geolocation;

namespace NexMuni.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        public List<Route> Routes
        {
            get
            {
                return routeList;
            }
            set
            {
                if (value != routeList)
                {
                    routeList = value;
                    RaisePropertyChanged();
                }
            }
        }

        public List<Stop> Stops
        {
            get
            {
                return stopsList;
            }
            set
            {
                if (value != stopsList)
                {
                    stopsList = value;
                    RaisePropertyChanged();
                }
            }
        }

        List<Route> routeList;
        List<Stop> stopsList;

        public bool RouteBoxEnabled
        {
            get
            {
                return routeBoxEnabled;
            }
            set
            {
                if(value != routeBoxEnabled)
                {
                    routeBoxEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool DirBoxEnabled
        {
            get
            {
                return dirBoxEnabled;
            }
            set
            {
                if (value != dirBoxEnabled)
                {
                    dirBoxEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool StopBoxEnabled
        {
            get
            {
                return stopBoxEnabled;
            }
            set
            {
                if (value != stopBoxEnabled)
                {
                    stopBoxEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        bool routeBoxEnabled = false;
        bool dirBoxEnabled = false;
        bool stopBoxEnabled = false;

        public Route SelectedRoute
        {
            get
            {
                return selectedRoute;
            }
            set
            {
                if(value != selectedRoute)
                {
                    selectedRoute = value;

                    if( selectedRoute == null)
                    {
                        SelectedDirection = null;
                    }
                    else
                    {
                        LoadRouteConfig();
                    }

                    DirBoxEnabled = true;
                    RaisePropertyChanged();
                }
            }
        }

        public RouteConfiguration RouteConfig
        {
            get
            {
                return routeConfig;
            }
            set
            {
                if (value != routeConfig)
                {
                    routeConfig = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Direction SelectedDirection
        {
            get
            {
                return selectedDirection;
            }
            set
            {
                if (value != selectedDirection)
                {
                    selectedDirection = value;

                    if (selectedDirection == null)
                        SelectedStop = null;
                    else
                        LoadStopsForDirection();

                    StopBoxEnabled = true;
                    RaisePropertyChanged();
                }
            }
        }

        public Stop SelectedStop
        {
            get
            {
                return selectedStop;
            }
            set
            {
                if (value != selectedStop)
                {
                    selectedStop = value;

                    if (selectedStop != null)
                        LoadTimesForStop();

                    RaisePropertyChanged();
                }
            }
        }       

        Route selectedRoute;
        RouteConfiguration routeConfig;
        Direction selectedDirection;
        Stop selectedStop;

        public Geopoint MapCenter
        {
            get { return mapCenter; }
            set
            {
                if(value != mapCenter)
                {
                    mapCenter = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string PredictionTimes
        {
            get { return predictionTimes; }
            set
            {
                if(value != predictionTimes)
                {
                    predictionTimes = value;
                    RaisePropertyChanged();
                }
            }
        }

        Geopoint mapCenter;
        string predictionTimes;

        public SearchViewModel()
        {
            MapCenter = new Geopoint(new BasicGeoposition { Latitude = 37.76463980133265, Longitude = -122.43809991881687 });
        }

        public async Task InitializeAsync()
        {
            WebHelper.Client.Agency = "sf-muni";
            Routes = await WebHelper.Client.GetRoutes();
            RouteBoxEnabled = true;
        }

        async void LoadRouteConfig()
        {
            RouteConfig = await WebHelper.Client.GetRouteConfig(SelectedRoute.Tag);
        }

        void LoadStopsForDirection()
        {
            var stops = new List<Stop>();

            foreach(var stop in SelectedDirection.Stops)
            {
                var s = RouteConfig.Stops.First(x => x.Tag == stop.Tag);
                stops.Add(s);
            }

            Stops = stops;
        }

        async void LoadTimesForStop()
        {
            var test = await WebHelper.Client.GetPredictionForStopTag(SelectedRoute.Tag, SelectedStop.Tag);

            var predictions = test?.Direction?.Predictions?.Take(3);
            var times = String.Empty;

            if (predictions == null)
            {
                PredictionTimes = "No currently running";
            }
            else
            {
                foreach (var p in predictions)
                {
                    times += p.Minutes + ",";
                }

                PredictionTimes = times;
            }
        }
    }
}
