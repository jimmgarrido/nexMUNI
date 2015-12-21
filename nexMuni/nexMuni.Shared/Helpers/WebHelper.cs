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

        private static readonly Dictionary<string, string> baseUrls = new Dictionary<string, string>()
        {
            {"multiPredictions","http://webservices.nextbus.com/service/publicXMLFeed?command=predictionsForMultiStops&a=sf-muni"},
            {"routeConfig","http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r="},
            {"searchPrediction", "http://webservices.nextbus.com/service/publicXMLFeed?command=predictions&a=sf-muni&stopId="},
            {"busLocations", "http://webservices.nextbus.com/service/publicXMLFeed?command=vehicleLocations&a=sf-muni&r="}
        };

        public static async Task<XDocument> GetMulitPredictionsAsync(string tags)
        {
            var builder = new StringBuilder();
            string[] splitTags = tags.Split(',');

            foreach (string tag in splitTags)
            {
                builder.Append("&stops=" + tag);
            }

            string url = baseUrls["multiPredictions"] + builder;

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
                await ErrorHandler.NetworkError("Error getting predictions. Check network connection and try again.");
                return null;
            }

            return xmlDoc;
        }

        public static async Task<XDocument> GetSearchPredictionsAsync(Stop stop, string route)
        {
            var url = baseUrls["searchPrediction"] + stop.stopId + "&routeTag=" + route.Substring(0, route.IndexOf('-'));

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
                await ErrorHandler.NetworkError("Error getting predictions. Check network connection and try again.");
                throw;
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

            var url = baseUrls["routeConfig"] + route;

            try
            {
                var response = await client.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();
                string reader = await response.Content.ReadAsStringAsync();
                xmlDoc = await Task.Run(() => XDocument.Parse(reader));
            }
            catch (Exception)
            {
                await ErrorHandler.NetworkError("Error getting route info. Check network connection and try again.");
                return null;
            }

            return xmlDoc;
        }

        public async static Task<XDocument> GetBusLocationsAsync(string route)
        {
            //Because the API requires an epoch time offset for some reason...
            TimeSpan epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            var timestamp = (long)epoch.TotalMilliseconds - TimeSpan.FromMinutes(5).TotalMilliseconds;

            if (route.Equals("Powell/Mason Cable Car")) route = "59";
            else if (route.Equals("Powell/Hyde Cable Car")) route = "60";
            else if (route.Equals("California Cable Car")) route = "61";
            else if (route.Contains('-'))
            {
                route = route.Substring(0, route.IndexOf('-'));
            }

            var url = baseUrls["busLocations"] + route + "&t=" + timestamp;

             try
            {
                var response = await client.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();
                string reader = await response.Content.ReadAsStringAsync();
                xmlDoc = await Task.Run(() => XDocument.Parse(reader));
            }
            catch (Exception)
            {
                await ErrorHandler.NetworkError("Error getting route info. Check network connection and try again.");
                return null;
            }

             return xmlDoc;
        }

        public async static Task<XDocument> GetRouteDirections(string route)
        {
            var client = new HttpClient();
            var response = new HttpResponseMessage();
            var xmlDoc = new XDocument();

            var url = baseUrls["routeConfig"] + route;

            //Make sure to pull from network not cache everytime predictions are refreshed 
            client.DefaultRequestHeaders.IfModifiedSince = DateTime.Now;
            try
            {
                response = await client.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();
                xmlDoc = await Task.Run(async () => XDocument.Parse(await response.Content.ReadAsStringAsync()));
            }
            catch (Exception)
            {
                await ErrorHandler.NetworkError("Error getting route information. Check your network connection and try again.");
                throw;
            }

            response.Dispose();
            client.Dispose();

            return xmlDoc;
        }
    }
}
