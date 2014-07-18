using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Net.Http;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace nexMuni
{
    class PredictionModel
    {
        public static async void GetXML(string url, StopData stop)
        {
#if WINDOWS_PHONE_APP
            var systemTray = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            systemTray.ProgressIndicator.Text = "Getting Arrival Times";
            systemTray.ProgressIndicator.ProgressValue = null;
#endif

            HttpClient client = new HttpClient();
            XDocument xmlDoc = new XDocument();
            Stream xmlStream;

            xmlStream = await client.GetStreamAsync(url);
            xmlDoc = XDocument.Load(xmlStream);

            GetPredictions(xmlDoc, stop);

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

            string outbound, inbound;
            string title;
            string[] outTimes = new string[3];
            string[] inTimes = new string[3];
            string time;
            int counter = 0;
            int j = 0, x, y;

            while(i < rootElements.Count())
            {
                if (i % 2 == 0)
                {
                    currElement = rootElements.ElementAt(i);
                    XAttribute titleAtt = currElement.Attribute("routeTitle");
                    title = titleAtt.ToString();
                    
                    x = title.IndexOf('-') + 1;
                    y = title.LastIndexOf('"');
                    title = title.Substring(x, y - x);

                    currElement = currElement.Element("direction");
                    if (currElement != null)
                    {
                        outbound = currElement.Attribute("title").ToString();
                        x = outbound.LastIndexOf('"');
                        outbound = outbound.Substring(7, x - 7);

                        predictionElements =
                            from e in currElement.Descendants("prediction")
                            select e;
                    }
                    else break;
                    
                    while (j < predictionElements.Count())
                    {
                        XElement element = predictionElements.ElementAt(j);
                        time = element.Attribute("minutes").ToString();
                        x = time.LastIndexOf('"');
                        time = time.Substring(9, x - 9);

                        if(j < 3)
                        {
                            outTimes[j] = time;
                            
                        }
                        j++;
                    }

                    StopDetailModel.routeList.Add(new RouteData(title, s.RoutesSplit[counter], outbound, outTimes));
                    
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
                    }
                    else break;

                    j = 0;
                    while (j < predictionElements.Count())
                    {
                        XElement element = predictionElements.ElementAt(j);
                        time = element.Attribute("minutes").ToString();
                        x = time.LastIndexOf('"');
                        time = time.Substring(9, x - 9);

                        if(j < 3)
                        {
                            inTimes[j] = time;
                            
                        }
                        j++;
                    }

                    StopDetailModel.routeList[counter].GetIn(inbound, inTimes);
                    counter++;
                }
                i++;
            }  
        }
    }
}
