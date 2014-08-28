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
        public static string URLstring {get; set;}
        public static StopData selectedStop { get; set; }
        public static HttpResponseMessage saved { get; set; }

        public static async void GetXML(string url, StopData stop)
        {
#if WINDOWS_PHONE_APP
            var systemTray = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            systemTray.ProgressIndicator.Text = "Getting Arrival Times";
            systemTray.ProgressIndicator.ProgressValue = null;
#endif
            URLstring = url;
            selectedStop = stop;

            var response = new HttpResponseMessage();
            var client = new HttpClient();
            XDocument xmlDoc = new XDocument();
            string reader;

            if (saved != null) response = saved;
            
            //Make sure to pull from network not cache everytime predictions are refreshed 
            response.Headers.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            client.DefaultRequestHeaders.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            if (response.Content != null) client.DefaultRequestHeaders.IfModifiedSince = response.Content.Headers.Expires;
            try
            {
                response = await client.GetAsync(new Uri(URLstring));
                response.EnsureSuccessStatusCode();
                response.Content.Headers.Expires = System.DateTime.Now;
                reader = await response.Content.ReadAsStringAsync();
                xmlDoc = XDocument.Parse(reader);

                saved = response;
                GetPredictions(xmlDoc, selectedStop);
            }
            catch(Exception ex)
            {
                ErrorHandler.NetworkError("Error getting predictions. Check network connection and try again.");
            }

#if WINDOWS_PHONE_APP
            systemTray.ProgressIndicator.ProgressValue = 0;
            systemTray.ProgressIndicator.Text = "nexMuni";    
#endif
        }

        private static void GetPredictions(XDocument doc, StopData s)
        {
            int i = 0;
            IEnumerable<XElement> rootElements =
                from e in doc.Descendants("predictions")
                select e;
            XElement currElement;
            IEnumerable<XElement> predictionElements;

            string dirTitle1, dirTitle2, title, time, route, fullTitle;
            string[] times1 = new string[4];
            string[] times2 = new string[4];
            int j, x, y;

            while(i < rootElements.Count())
            {
                currElement = rootElements.ElementAt(i);
                fullTitle = currElement.Attribute("routeTitle").ToString();

                if (fullTitle.Contains('-'))
                {
                    x = fullTitle.IndexOf('-');   
                    y = fullTitle.LastIndexOf('"');
                    title = fullTitle.Substring(x + 1, y - (x + 1));
                    y = fullTitle.IndexOf('"');
                    route = fullTitle.Substring(y + 1, x - (y + 1));
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
                        dirTitle1 = currElement.Attribute("title").ToString();
                        x = dirTitle1.LastIndexOf('"');
                        dirTitle1 = dirTitle1.Substring(7, x - 7);

                        predictionElements =
                            from e in currElement.Descendants("prediction")
                            select e;

                        j = 0;
                        //times1 = new string[3];
                        while (j < predictionElements.Count())
                        {
                            
                            XElement element = predictionElements.ElementAt(j);
                            time = element.Attribute("minutes").ToString();
                            x = time.LastIndexOf('"');
                            time = time.Substring(9, x - 9);

                            if (j < 4) times1[j] = time;
                            j++;
                        }
                        StopDetailModel.routeList.Add(new RouteData(title, route, dirTitle1, times1));
                    }  
                }
                else
                {
                    currElement = rootElements.ElementAt(i).Element("direction");

                    if (currElement != null)
                    {
                        dirTitle2 = currElement.Attribute("title").ToString();
                        x = dirTitle2.LastIndexOf('"');
                        dirTitle2 = dirTitle2.Substring(7, x - 7);

                        predictionElements =
                            from e in currElement.Descendants("prediction")
                            select e;

                        j = 0;
                        //times2 = new string[3];
                        while (j < predictionElements.Count())
                        {
                            XElement element = predictionElements.ElementAt(j);
                            time = element.Attribute("minutes").ToString();
                            x = time.LastIndexOf('"');
                            time = time.Substring(9, x - 9);

                            if (j < 4) times2[j] = time;
                            j++;
                        }
                        
                        foreach (RouteData r in StopDetailModel.routeList)
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

        private static void GetSearchPredictions(XDocument doc)
        {
            string [] searchTimes = new string[5];
            int i = 0;
            string times = null;

            IEnumerable<XElement> elements =
                from e in doc.Descendants("predictions").Descendants("direction").Descendants("prediction")
                select e;

            foreach (XElement el in elements)
            {
                if(i < 5)
                {
                    searchTimes[i] = el.Attribute("minutes").Value;
                    i++;
                }  
            }

            i = 0;

            while(i < searchTimes.Length && searchTimes[i] != null)
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
            
            MainPage.searchText.Text = times;
        }

        internal static void UpdateTimes()
        {
            StopDetailModel.routeList.Clear();
            GetXML(URLstring, selectedStop);
        }

        internal static async void SearchPredictions(Stop selectedStop, string route, string url)
        {
            var response = new HttpResponseMessage();
            var client = new HttpClient();
            XDocument xmlDoc = new XDocument();
            string reader;

            if (saved != null) response = saved;

            //Make sure to ppull from network not cache everytime predictions are refreshed 
            response.Headers.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            client.DefaultRequestHeaders.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            if (response.Content != null) client.DefaultRequestHeaders.IfModifiedSince = response.Content.Headers.Expires;
            try
            {
                response = await client.GetAsync(new Uri(url));
                response.Content.Headers.Expires = System.DateTime.Now;

                reader = await response.Content.ReadAsStringAsync();
                xmlDoc = XDocument.Parse(reader);

                saved = response;
                GetSearchPredictions(xmlDoc);
            }
            catch (Exception ex)
            {
                ErrorHandler.NetworkError("Error getting predictions. Check network connection and try again.");
            }   
        }
    }
}
