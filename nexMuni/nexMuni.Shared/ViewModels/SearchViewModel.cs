using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using nexMuni.Helpers;
using nexMuni.ViewModels;
using nexMuni.Views;
using nexMuni.DataModels;

namespace nexMuni.ViewModels
{
    public class SearchViewModel
    {
        //public static bool IsDataLoaded { get; set; }
        //public static ObservableCollection<string> DirectionsList { get; set; }
        //public static ObservableCollection<StopData> stopsList { get; set; }
        //public static List<string> outboundStops = new List<string>();
        //public static List<string> inboundStops = new List<string>();
        
        //public static StopData foundStop;
        //private static string selectedRoute;
        public static Stop selectedStop { get; set; }

        public List<string> RoutesList { get; set; }

        private Task initialization;

        public SearchViewModel()
        {
            initialization = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            RoutesList = await DatabaseHelper.QueryForRoutes();
        }

//        public static async void LoadData()
//        {
//            await LoadRoutes();

//            DirectionsList = new ObservableCollection<string>();
//            stopsList = new ObservableCollection<StopData>();

//            MainPage.dirComboBox.ItemsSource = DirectionsList;
//            MainPage.stopPicker.ItemsSource = stopsList;

//            MainPage.routePicker.ItemsPicked += RouteSelected;
//            MainPage.dirComboBox.SelectionChanged += DirSelected;
//            MainPage.stopPicker.ItemsPicked += StopSelected;

//        }

//        public static async Task LoadRoutes()
//        {
//            MainPage.routePicker.ItemsSource = await DatabaseHelper.QueryForRoutes();
//            MainPage.routeBtn.IsEnabled = true;
//        }

//        public static async void RouteSelected(ListPickerFlyout sender, ItemsPickedEventArgs args)
//        {
//            #if WINDOWS_PHONE_APP
//                        var systemTray = StatusBar.GetForCurrentView();
//                        systemTray.ProgressIndicator.ProgressValue = null;
//            #endif

//            selectedRoute = sender.SelectedItem.ToString();
//            MainPage.routeBtn.Content = selectedRoute;
//            MainPage.routePicker.SelectedIndex = sender.SelectedIndex;
//            MainPage.timesText.Visibility = Visibility.Collapsed;
//            MainPage.favSearchBtn.Visibility = Visibility.Collapsed;

//            if (DirectionsList.Count != 0)
//            {
//                MainPage.dirComboBox.SelectedIndex = -1;
//                DirectionsList.Clear();
//            }
//            if (stopsList.Count != 0)
//            {
//                MainPage.stopPicker.SelectedIndex = -1;
//                stopsList.Clear();
//            }

//            await LoadDirections(selectedRoute);

//            MainPage.dirText.Visibility = Visibility.Visible;
//            MainPage.dirComboBox.Visibility = Visibility.Visible;

//            #if WINDOWS_PHONE_APP
//                        systemTray.ProgressIndicator.ProgressValue = 0;
//            #endif
//        }

//        private static async Task LoadDirections(string _route)
//        {
//            string dirURL = "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r=";

//            if (_route.Equals("Powell/Mason Cable Car")) _route = "59";
//            else if (_route.Equals("Powell/Hyde Cable Car")) _route = "60";
//            else if (_route.Equals("California Cable Car")) _route = "61";
//            else
//            {
//                _route = _route.Substring(0, _route.IndexOf('-'));
//            }
            
//            //selectedRoute = _route;
//            dirURL = dirURL + _route;

//            var response = new HttpResponseMessage();
//            var client = new HttpClient();
//            XDocument xmlDoc = new XDocument();
//            string reader;

//            //Make sure to pull from network not cache everytime predictions are refreshed 
//            client.DefaultRequestHeaders.IfModifiedSince = DateTime.Now;
//            try
//            {
//                response = await client.GetAsync(new Uri(dirURL));

//                reader = await response.Content.ReadAsStringAsync();

//                GetDirections(XDocument.Parse(reader));  
//            }
//            catch(Exception)
//            {
//                ErrorHandler.NetworkError("Error getting route information. Please try again.");
//            }            
//        }

//        private static void GetDirections(XDocument doc)
//        {
//            IEnumerable<XElement> tagElements;
//            IEnumerable<XElement> rootElement =
//                from e in doc.Descendants("route")
//                select e;
//            IEnumerable<XElement> elements = 
//                from d in rootElement.ElementAt(0).Elements("stop")
//                select d;

//            //Add all route's stops to a collection
//            foreach (XElement el in elements)
//            {
//                allStopsList.Add(new StopData(el.Attribute("title").Value,
//                                              el.Attribute("stopId").Value, 
//                                              el.Attribute("tag").Value, 
//                                              el.Attribute("lon").Value, 
//                                              el.Attribute("lat").Value));
//            }

//            //Move to direction element
//            elements =
//                from d in rootElement.ElementAt(0).Elements("direction")
//                select d;

//            foreach (XElement el in elements)
//            {   
//                //Add direction title
//                DirectionsList.Add(el.Attribute("title").Value);

//                if(el.Attribute("name").Value == "Inbound")
//                {
//                    //Get all stop elements under direction element
//                    tagElements =
//                        from x in el.Elements("stop")
//                        select x;

//                    if (inboundStops.Count != 0) inboundStops.Clear();
//                    //Add tags for direction to a collection
//                    foreach (XElement y in tagElements)
//                    {
//                        inboundStops.Add(y.Attribute("tag").Value);
//                    }
//                } else if (el.Attribute("name").Value == "Outbound")
//                {
//                    //Get all stop elements under direction element
//                    tagElements =
//                        from x in el.Elements("stop")
//                        select x;

//                    if (outboundStops.Count != 0) outboundStops.Clear();
//                    //Add tags for direction to a collection
//                    foreach (XElement y in tagElements)
//                    {
//                        outboundStops.Add(y.Attribute("tag").Value);
//                    }
//                }
//            }
//            MainPage.dirComboBox.SelectedIndex = 0;

//            MapRouteView(doc);
//        }

//        private static async void MapRouteView(XDocument doc)
//        {
//            await MainPage.searchMap.TrySetViewAsync(new Geopoint(new BasicGeoposition() { Latitude = 37.7599, Longitude = -122.437 }), 11.5);
//            List<BasicGeoposition> positions = new List<BasicGeoposition>();
//            IEnumerable<XElement> subElements;
//            List<MapPolyline> route = new List<MapPolyline>();

//            IEnumerable<XElement> rootElement =
//                from e in doc.Descendants("route")
//                select e;
//            IEnumerable<XElement> elements =
//                from d in rootElement.ElementAt(0).Elements("path")
//                select d;
//            int x = 0;
//            if (MainPage.searchMap.MapElements.Count > 0) MainPage.searchMap.MapElements.Clear();
//            foreach (XElement el in elements)
//            {
//                subElements =
//                    from p in el.Elements("point")
//                    select p;

//                if (positions.Count > 0) positions.Clear();
//                foreach (XElement e in subElements)
//                {
//                    positions.Add(new BasicGeoposition() { Latitude = Double.Parse(e.Attribute("lat").Value), Longitude = Double.Parse(e.Attribute("lon").Value) });
//                }
//                route.Add(new MapPolyline());
//                route[x].StrokeColor = Color.FromArgb(255,179,27,27);
//                route[x].StrokeThickness = 2.00;
//                route[x].ZIndex = 99;
//                route[x].Path = new Geopath(positions);
//                route[x].Visible = true;
//                MainPage.searchMap.MapElements.Add(route[x]);
//                x++;
//            }

//            if (LocationHelper.phoneLocation != null)
//            {
//                Image icon = new Image
//                {
//                    Source = new BitmapImage(new Uri("ms-appx:///Assets/Location.png")),
//                    Width = 25,
//                    Height = 25
//                };

//                MainPage.searchMap.Children.Add(icon);
//                MapControl.SetNormalizedAnchorPoint(icon, new Point(0.5, 0.5));
//                MapControl.SetLocation(icon, LocationHelper.phoneLocation.Coordinate.Point);
//            }
//        }

//        public static void DirSelected(object sender, SelectionChangedEventArgs e)
//        {
//            if(((ComboBox)sender).SelectedIndex != -1)
//            {
//                MainPage.stopPicker.SelectedIndex = -1;
//                string selectedDir = ((ComboBox)sender).SelectedItem.ToString();
//                LoadStops(selectedDir);
//            }
//        }

//        private static void LoadStops(string _dir)
//        {
//            if(stopsList.Count != 0) stopsList.Clear();

//            if(_dir.Contains("Inbound"))
//            {
//                foreach(string s in inboundStops)
//                {
//                    foundStop = allStopsList.Find(z => z.Tags == s);
//                    //stopsList.Add(new StopData(foundStop.Name, foundStop.StopID, foundStop.Tags, foundStop.Lon.ToString(), foundStop.Lat.ToString()));
//                    stopsList.Add(foundStop);
//                }
//            }
//            else if(_dir.Contains("Outbound"))
//            {
//                foreach(string s in outboundStops)
//                {
//                    foundStop = allStopsList.Find(z => z.Tags == s);
//                    //stopsList.Add(new StopData(FoundStops[0].title, FoundStops[0].stopID, FoundStops[0].tag, FoundStops[0].lon.ToString(), FoundStops[0].lat.ToString()));
//                    stopsList.Add(foundStop);
//                }
//            }

//            MainPage.stopBtn.IsEnabled = true;
//            MainPage.stopBtn.Content = String.Empty;
//            MainPage.stopText.Visibility = Visibility.Visible;
//            MainPage.stopBtn.Visibility = Visibility.Visible;
//            MainPage.timesText.Visibility = Visibility.Collapsed;
//            MainPage.favSearchBtn.Visibility = Visibility.Collapsed;
//        }

//        public static async void StopSelected(ListPickerFlyout sender, ItemsPickedEventArgs args)
//        {
//            if (sender.SelectedIndex != -1)
//            {
//#if WINDOWS_PHONE_APP
//                var systemTray = StatusBar.GetForCurrentView();
//                systemTray.ProgressIndicator.Text = "Getting Arrival Times";
//                systemTray.ProgressIndicator.ProgressValue = null;
//#endif

//                selectedStop = sender.SelectedItem as StopData;
//                string title = selectedStop.Name;
//                MainPage.stopBtn.Content = title;

//                if (title.Contains("Inbound"))
//                {
//                    title = title.Replace(" Inbound", "");
//                }
//                if (title.Contains("Outbound"))
//                {
//                    title = title.Replace(" Outbound", "");
//                }

//                string[] temp = selectedStop.Name.Split('&');
//                string reversed;
//                if (temp.Count() > 1)
//                {
//                    reversed = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));
//                }
//                else reversed = "";

//                string timesURL = "http://webservices.nextbus.com/service/publicXMLFeed?command=predictions&a=sf-muni&stopId=" + selectedStop.StopID + "&routeTag=" + selectedRoute.Substring(0, selectedRoute.IndexOf('-'));

//                //await MainPage.searchMap.TrySetViewAsync(new Geopoint(new BasicGeoposition() { Latitude = selectedStop.Lat, Longitude = selectedStop.Lon }), 16.5);

//                if (MainPage.timesText.Visibility == Visibility.Visible) MainPage.timesText.Visibility = Visibility.Collapsed;
//                if (MainPage.favSearchBtn.Visibility == Visibility.Visible) MainPage.favSearchBtn.Visibility = Visibility.Collapsed;

//                //Check to see if the stop is in user's favorites list
//                if (MainPageModel.FavoritesStops.Any(z => z.Name == title || z.Name == reversed))
//                {
//                    foreach (StopData s in MainPageModel.FavoritesStops)
//                    {
//                        if (s.Name == title) selectedStop.FavID = s.FavID;
//                    }
//                    MainPage.timesText.Visibility = Visibility.Visible;
//                    MainPage.favSearchBtn.Visibility = Visibility.Collapsed;
//                    MainPage.removeSearchBtn.Visibility = Visibility.Visible;
//                }
//                else
//                {
//                    MainPage.timesText.Visibility = Visibility.Visible;
//                    MainPage.favSearchBtn.Visibility = Visibility.Visible;
//                    MainPage.removeSearchBtn.Visibility = Visibility.Collapsed;
//                }

//                //Get bus predictions for stop
//                PredictionHelper.GetSearchTimes(await PredictionHelper.GetXml(timesURL));

//#if WINDOWS_PHONE_APP
//                systemTray.ProgressIndicator.ProgressValue = 0;
//                systemTray.ProgressIndicator.Text = "nexMUNI";
//#endif
//            }
//        }
    }
}
