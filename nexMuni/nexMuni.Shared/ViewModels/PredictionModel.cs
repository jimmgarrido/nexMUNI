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

namespace nexMuni
{
    class PredictionModel
    {
        private static string URLstring {get; set;}

        public static async Task<XDocument> GetXML(string url)
        {
            URLstring = url;

            var response = new HttpResponseMessage();
            var client = new HttpClient();
            XDocument xmlDoc = new XDocument();
            string reader;

            //Make sure to pull from network not cache everytime predictions are refreshed 
            client.DefaultRequestHeaders.IfModifiedSince = System.DateTime.Now;
            try
            {
                response = await client.GetAsync(new Uri(URLstring));
                response.EnsureSuccessStatusCode();
                reader = await response.Content.ReadAsStringAsync();
                xmlDoc = XDocument.Parse(reader);

                //GetTimes(xmlDoc);
            }
            catch(Exception)
            {
                ErrorHandler.NetworkError("Error getting predictions. Check network connection and try again.");
            }

            return xmlDoc;
        }

        public static void GetTimes(XDocument doc)
        {
            int i = 0;
            IEnumerable<XElement> rootElements =
                from e in doc.Descendants("predictions")
                select e;
            XElement currElement, element;
            IEnumerable<XElement> predictionElements;

            string dirTitle1, dirTitle2, title, time, route, fullTitle;
            string[] times1 = new string[4];
            string[] times2 = new string[4];

            int j=0, x;


            while(i < rootElements.Count())
            {
                currElement = rootElements.ElementAt(i);
                fullTitle = currElement.Attribute("routeTitle").Value;

                if (fullTitle.Contains('-'))
                {
                    x = fullTitle.IndexOf('-');   
                    title = fullTitle.Substring(x + 1, fullTitle.Length - (x + 1));
                    route = fullTitle.Substring(0, x );
                } else
                {
                    x = fullTitle.IndexOf('"');
                    title = fullTitle.Substring(x + 1, (fullTitle.Length - (x+2)));
                    route = currElement.Attribute("routeTag").ToString();
                    route = route.Substring(10, route.Length - 11);
                }

                //Check to see if the route has already been added to the collection
                if (!StopDetailModel.routeList.Any(z => z.RouteNum == route))
                {          
                    currElement = currElement.Element("direction");
                    if (currElement != null)
                    {
                        dirTitle1 = currElement.Attribute("title").Value;;

                        predictionElements =
                            from e in currElement.Descendants("prediction")
                            select e;

                        j = 0;

                        while (j < predictionElements.Count())
                        {                           
                            element = predictionElements.ElementAt(j);
                            time = element.Attribute("minutes").Value;

                            if (j < 4) times1[j] = time;
                            j++;
                        }
                        StopDetailModel.routeList.Add(new Route(title, route, dirTitle1, times1));
                    }  
                }
                else
                {
                    currElement = rootElements.ElementAt(i).Element("direction");

                    if (currElement != null)
                    {
                        dirTitle2 = currElement.Attribute("title").Value ;

                        predictionElements =
                            from e in currElement.Descendants("prediction")
                            select e;

                        j = 0;

                        while (j < predictionElements.Count())
                        {
                            element = predictionElements.ElementAt(j);
                            time = element.Attribute("minutes").Value;

                            if (j < 4) times2[j] = time;
                            j++;
                        }
                        
                        foreach (Route r in StopDetailModel.routeList)
                        {
                            if (r.RouteNum == route)
                            {
                                r.AddDir2(dirTitle2, times2);
                            }
                        }         
                    }     
                }
                i++;
            }
            if (StopDetailModel.routeList.Count == 0) StopDetail.noTimeText.Visibility = Windows.UI.Xaml.Visibility.Visible;
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

        internal static async void UpdateTimes()
        {
            GetTimes(await GetXML(URLstring));
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
