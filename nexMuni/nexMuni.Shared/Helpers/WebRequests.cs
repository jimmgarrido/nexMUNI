using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni.Helpers
{
    public class WebRequests
    {
        private static Dictionary<string, string> URLs = new Dictionary<string, string>()
        {
            {"multiPredictions","http://webservices.nextbus.com/service/publicXMLFeed?command=predictionsForMultiStops&a=sf-muni"},
            {"routeConfig","http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r="},
        };

        public static string GetMulitPredictionUrl(string tags)
        {
            StringBuilder cont = new StringBuilder();
            string[] splitTags = tags.Split(',');

            foreach(string t in splitTags)
            {
                cont.Append("&stops=" + t);
            }

            return URLs["multiPredictions"] + cont.ToString();
        }
    }
}
