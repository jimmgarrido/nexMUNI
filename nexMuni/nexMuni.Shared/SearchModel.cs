using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace nexMuni
{
    class SearchModel
    {
        public static bool IsDataLoaded { get; set; }
        public static ObservableCollection<string> RoutesCollection { get; set; }
        public static ObservableCollection<string> DirCollection { get; set; }
        public static ObservableCollection<string> StopCollection { get; set; }
        public static HttpResponseMessage saved { get; set; }
        public static List<Routes> RoutesList;
        public static List<BusStop> StopsList;

        public static void LoadStops()
        {
            RoutesList = DatabaseHelper.QueryForRoutes();
            RoutesCollection = new ObservableCollection<string>();

            if (DirCollection == null) DirCollection = new ObservableCollection<string>();

            foreach (Routes s in RoutesList)
            {
                RoutesCollection.Add(s.Title);
            }

            IsDataLoaded = true;
        }

        public static void RouteSelected(object sender, SelectionChangedEventArgs e)
        {
            string selectedRoute = ((ComboBox)sender).SelectedItem.ToString();
            LoadDirections(selectedRoute);
        }

        private static async void LoadDirections(string _route)
        {
            string URL = "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r=";
            int i = _route.IndexOf('-');
            _route = _route.Substring(0, i);
            URL = URL + _route;

            var response = new HttpResponseMessage();
            var client = new HttpClient();
            XDocument xmlDoc = new XDocument();
            string reader;

            if (saved != null) response = saved;

            //Make sure to ppull from network not cache everytime predictions are refreshed 
            response.Headers.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            client.DefaultRequestHeaders.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            if (response.Content != null) client.DefaultRequestHeaders.IfModifiedSince = response.Content.Headers.Expires;
            response = await client.GetAsync(new Uri(URL));
            response.Content.Headers.Expires = System.DateTime.Now;

            saved = response;

            reader = await response.Content.ReadAsStringAsync();
            xmlDoc = XDocument.Parse(reader);

            GetDirections(xmlDoc);            
        }

        private static void GetDirections(XDocument doc)
        {
            IEnumerable<XElement> rootElement =
                from e in doc.Descendants("route")
                select e;
            IEnumerable<XElement> elements = 
                from d in rootElement.ElementAt(0).Elements("direction")
                select d;

            DirCollection.Clear();
            foreach (XElement el in elements)
            {
                DirCollection.Add(el.Attribute("title").Value);
            }
        }

        public static void DirSelected(object sender, SelectionChangedEventArgs e)
        {
            string selectedDir = ((ComboBox)sender).SelectedItem.ToString();
            LoadStops(selectedDir);
        }

        private static void LoadStops(string _dir)
        {
            StopsList = DatabaseHelper.QueryForStops();
        }
    }
}
