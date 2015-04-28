using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Windows.UI.Popups;
using nexMuni.DataModels;

namespace nexMuni.Helpers
{
    class PredictionHelper
    {
        public static async Task<List<Route>> GetPredictionTimes(string url)
        {
            return GetPredictions(await GetXml(url));
        }
        public static async Task<XDocument> GetXml(string url)
        {
            var response = new HttpResponseMessage();
            var client = new HttpClient();
            XDocument xmlDoc = new XDocument();
            string reader;

            //Make sure to pull from network not cache everytime predictions are refreshed 
            client.DefaultRequestHeaders.IfModifiedSince = System.DateTime.Now;
            try
            {
                response = await client.GetAsync(new Uri(url));
                response.EnsureSuccessStatusCode();
                reader = await response.Content.ReadAsStringAsync();
                xmlDoc = XDocument.Parse(reader);
            }
            catch(Exception)
            {
                ErrorHandler.NetworkError("Error getting predictions. Check network connection and try again.");
            }

            return xmlDoc;
        }

        private static List<Route> GetPredictions(XDocument doc)
        {
            IEnumerable<XElement> rootElements = doc.Element("body").Elements("predictions");
            XElement subElement;
            List<Route> routes = new List<Route>();

            string routeTitle, routeNum, fullTitle;
            string[] times1 = new string[4];
            string[] times2 = new string[4];

            foreach(XElement currentElement in rootElements)
            {
                int index;
                fullTitle = currentElement.Attribute("routeTitle").Value;

                if (fullTitle.Contains('-'))
                {
                    index = fullTitle.IndexOf('-');   
                    routeTitle = fullTitle.Substring(index + 1, fullTitle.Length - (index + 1));
                    routeNum = fullTitle.Substring(0, index );
                } else
                {
                    index = fullTitle.IndexOf('"');
                    routeTitle = fullTitle.Substring(index + 1, (fullTitle.Length - (index+2)));
                    routeNum = currentElement.Attribute("routeTag").Value;
                }

                //Check to see if the route has already been added to the collection
                if (!routes.Any(z => z.RouteNumber == routeNum))
                {
                    Route tempRoute = new Route(routeTitle, routeNum);
                    
                    subElement = currentElement.Element("direction");
                    if (subElement != null)
                    {
                        tempRoute.Directions.Add(GetTimes(subElement));
                        routes.Add(tempRoute);
                    }
                    //{
                    //    int j = 0;
                    //    dirTitle1 = subElement.Attribute("title").Value;

                    //    predictionElements = subElement.Elements("prediction");
                    //    foreach (XElement element in predictionElements)
                    //    {                           
                    //        time = element.Attribute("minutes").Value;

                    //        if (j < 4) times1[j] = time;
                    //        j++;
                    //    }
                    //    routes.Add(new Route(routeTitle, routeNum, dirTitle1, times1));
                    //}  
                }
                else
                {
                    Route tempRoute = routes.Find(r => r.RouteNumber == routeNum);

                    subElement = currentElement.Element("direction");
                    if (subElement != null)
                    {
                        tempRoute.Directions.Add(GetTimes(subElement));
                    }
                    //{
                    //    int j = 0;
                    //    dirTitle2 = subElement.Attribute("title").Value;

                    //    predictionElements = subElement.Elements("prediction");
                    //    foreach (XElement element in predictionElements)
                    //    {
                    //        time = element.Attribute("minutes").Value;

                    //        if (j < 4) times2[j] = time;
                    //        j++;
                    //    }
                        
                    //    foreach (Route r in routes)
                    //    {
                    //        if (r.RouteNumber == routeNum)
                    //        {
                    //            r.AddDir2(dirTitle2, times2);
                    //        }
                    //    }         
                    //}     
                }   
            }
            return routes;
        }

        private static RouteDirection GetTimes(XElement element)
        {
            int maxTimes;
            StringBuilder builder = new StringBuilder();
            RouteDirection tempDirection = new RouteDirection(element.Attribute("title").Value);
            IEnumerable<XElement> predictionElements = element.Elements("prediction");

            if (predictionElements.Count() < 4)
                maxTimes = predictionElements.Count();
            else
                maxTimes = 4;
            
            for (int i = 0; i < maxTimes; i++)
            {
                if (i == maxTimes - 1)
                    builder.Append(predictionElements.ElementAt(i).Attribute("minutes").Value + " mins");
                else
                    builder.Append(predictionElements.ElementAt(i).Attribute("minutes").Value + ", ");
            }

            tempDirection.Times = builder.ToString();
            return tempDirection;
        }

        public static void GetSearchTimes(XDocument doc)
        {
            string[] searchTimes = new string[5];
            int i = 0;
            string times = null;

            IEnumerable<XElement> elements =
                from e in doc.Descendants("predictions").Descendants("direction").Descendants("prediction")
                select e;

            foreach (XElement el in elements)
            {
                if (i < 5)
                {
                    searchTimes[i] = el.Attribute("minutes").Value;
                    i++;
                }
            }

            i = 0;

            while (i < searchTimes.Length && searchTimes[i] != null)
            {
                if (i == 0)
                {
                    times = searchTimes[0];
                    i++;
                }
                else if (searchTimes[i] != null)
                {
                    times = times + ", " + searchTimes[i];
                    i++;
                }
            }

            if (times == null) times = "No busses at this time";
            else times = times + " mins";

            MainPage.timesText.Text = times;
        }

        //internal static async Task SearchPredictions(string route, string url)
        //{
        //    var response = new HttpResponseMessage();
        //    var client = new HttpClient();
        //    XDocument xmlDoc = new XDocument();
        //    string reader;

        //    //Make sure to pull from network not cache everytime predictions are refreshed 
        //    client.DefaultRequestHeaders.IfModifiedSince = System.DateTime.Now;
        //    try
        //    {
        //        response = await client.GetAsync(new Uri(url));

        //        reader = await response.Content.ReadAsStringAsync();

        //        GetTimes(XDocument.Parse(reader));

        //    }
        //    catch (Exception)
        //    {
        //        ErrorHandler.NetworkError("Error getting predictions. Check network connection and try again.");
        //    }   
        //}
    }
}
