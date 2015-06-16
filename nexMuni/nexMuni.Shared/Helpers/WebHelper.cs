using nexMuni.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;
using Windows.Web.Http;

namespace nexMuni.Helpers
{
    public class WebHelper
    {
        private static HttpClient client = new HttpClient();
        private static XDocument xmlDoc = new XDocument();

        private static Dictionary<string, string> Urls = new Dictionary<string, string>()
        {
            {"multiPredictions","http://webservices.nextbus.com/service/publicXMLFeed?command=predictionsForMultiStops&a=sf-muni"},
            {"routeConfig","http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r="},
            {"searchPrediction", "http://webservices.nextbus.com/service/publicXMLFeed?command=predictions&a=sf-muni&stopId="}
        };

        public static async Task<XDocument> GetMulitPredictionsAsync(string tags)
        {
            StringBuilder builder = new StringBuilder();
            string[] splitTags = tags.Split(',');

            foreach (string tag in splitTags)
            {
                builder.Append("&stops=" + tag);
            }

            var url = Urls["multiPredictions"] + builder.ToString();

            //Make sure to pull from the network and not cache everytime predictions are refreshed 
            client.DefaultRequestHeaders.IfModifiedSince = DateTime.Now;
            try
            {
                var response = await client.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();
                var reader = await response.Content.ReadAsStringAsync();
                xmlDoc = XDocument.Parse(reader);
            }
            catch (Exception)
            {
                ErrorHandler.NetworkError("Error getting predictions. Check network connection and try again.");
                return null;
            }

            return xmlDoc;
        }

        public static async Task<XDocument> GetSearchPredictionsAsync(Stop stop, string route)
        {
            var url = Urls["searchPrediction"] + stop.stopId + "&routeTag=" + route.Substring(0, route.IndexOf('-'));

             //Make sure to pull from the network and not cache everytime predictions are refreshed 
            client.DefaultRequestHeaders.IfModifiedSince = DateTime.Now;
            try
            {
                var response = await client.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();
                var reader = await response.Content.ReadAsStringAsync();
                xmlDoc = XDocument.Parse(reader);
            }
            catch (Exception)
            {
                ErrorHandler.NetworkError("Error getting predictions. Check network connection and try again.");
                return null;
            }

            return xmlDoc;
        }

        public static async Task<XDocument> GetRoutePathAsync(string route)
        {
            if (route.Equals("Powell/Mason Cable Car")) route = "59";
            else if (route.Equals("Powell/Hyde Cable Car")) route = "60";
            else if (route.Equals("California Cable Car")) route = "61";
            else if (route.Contains('-'))
            {
                route = route.Substring(0, route.IndexOf('-'));
            }

            var url = Urls["routeConfig"] + route;

            try
            {
                var response = await client.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();
                string reader = await response.Content.ReadAsStringAsync();
                xmlDoc = XDocument.Parse(reader);
            }
            catch (Exception)
            {
                ErrorHandler.NetworkError("Error getting route info. Check network connection and try again.");
                return null;
            }

            return xmlDoc;
        }
    }
}
