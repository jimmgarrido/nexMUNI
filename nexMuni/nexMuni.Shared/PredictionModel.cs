using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

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
            
            response.Headers.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            client.DefaultRequestHeaders.CacheControl.Add(new HttpNameValueHeaderValue("max-age", "1"));
            if (response.Content != null) client.DefaultRequestHeaders.IfModifiedSince = response.Content.Headers.Expires;
            response = await client.GetAsync(new Uri(URLstring));
            response.Content.Headers.Expires = System.DateTime.Now;
            
            saved = response;

            reader = await response.Content.ReadAsStringAsync();
            xmlDoc = XDocument.Parse(reader);

            GetPredictions(xmlDoc, selectedStop);

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

            string outbound, inbound, title, currTag ="0", time;
            string[] outTimes = new string[3];
            string[] inTimes = new string[3];
            int counter = 0, j, x, y;

            while(i < rootElements.Count())
            {
                currElement = rootElements.ElementAt(i);
                XAttribute titleAtt = currElement.Attribute("routeTitle");
                title = titleAtt.ToString();
                x = title.IndexOf('-') + 1;
                y = title.LastIndexOf('"');
                title = title.Substring(x, y - x);

                if (!currElement.Attribute("routeTag").ToString().Equals(currTag))
                {          
                    currTag = currElement.Attribute("routeTag").ToString();
                    currElement = currElement.Element("direction");
                    if (currElement != null)
                    {
                        outbound = currElement.Attribute("title").ToString();
                        x = outbound.LastIndexOf('"');
                        outbound = outbound.Substring(7, x - 7);

                        predictionElements =
                            from e in currElement.Descendants("prediction")
                            select e;

                        j = 0;
                        outTimes = new string[3];
                        while (j < predictionElements.Count())
                        {
                            
                            XElement element = predictionElements.ElementAt(j);
                            time = element.Attribute("minutes").ToString();
                            x = time.LastIndexOf('"');
                            time = time.Substring(9, x - 9);

                            if (j < 3) outTimes[j] = time;
                            j++;
                        }
                        StopDetailModel.routeList.Add(new RouteData(title, s.RoutesSplit[counter], outbound, outTimes));
                        counter++;
                    }  
                }
                else
                {
                    currElement = rootElements.ElementAt(i).Element("direction");

                    if (currElement != null)
                    {
                        inbound = currElement.Attribute("title").ToString();
                        x = inbound.LastIndexOf('"');
                        inbound = inbound.Substring(7, x - 7);

                        predictionElements =
                            from e in currElement.Descendants("prediction")
                            select e;

                        j = 0;
                        inTimes = new string[3];
                        while (j < predictionElements.Count())
                        {
                            XElement element = predictionElements.ElementAt(j);
                            time = element.Attribute("minutes").ToString();
                            x = time.LastIndexOf('"');
                            time = time.Substring(9, x - 9);

                            if (j < 3) inTimes[j] = time;
                            j++;
                        }
                        StopDetailModel.routeList[counter - 1].AddDir2(inbound, inTimes); 
                    }     
                }
                i++;
            }  
        }

        internal static void UpdateTimes()
        {
            StopDetailModel.routeList.Clear();
            GetXML(URLstring, selectedStop);
        }
    }
}
