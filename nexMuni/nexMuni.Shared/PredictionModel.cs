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
        public static void SendToModel(ObservableCollection<RouteData> list)
        {
            //GetXML(StopDetailModel.url);
        }

        public static async void GetXML(string url, StopData stop)
        {
            HttpClient client = new HttpClient();
            XDocument xmlDoc = new XDocument();
            Stream xmlStream;

            xmlStream = await client.GetStreamAsync(url);
            xmlDoc = XDocument.Load(xmlStream);

            GetPredictions(xmlDoc, stop);
        }

        private static void GetPredictions(XDocument doc, StopData s)
        {
            XNode node = doc.Root.FirstNode;
            XElement el = node as XElement;

            string outbound, inbound;
            string title;
            string[] outTimes = new string[3];
            string[] inTimes = new string[3];
            string time;
            bool outDir = true;
            int counter = 0;

            int i = 0;
            while(i < s.RoutesSplit.Length)
            {
                if (outDir)
                {
                    title = el.Attribute("routeTitle").ToString();

                    //node = node.NextNode;
                    el = el.Element("direction");
                    outbound = el.Attribute("title").ToString();

                    int j = 0;
                    while(j < 1)
                    {
                        el = el.Element("prediction");
                        //el = node as XElement;
                        time = el.Attribute("minutes").ToString();

                        outTimes[i] = time;
                        j++;
                    }

                    outDir = false;
                    el = node.NextNode as XElement;
                    StopDetailModel.routeList.Add(new RouteData(title.Substring(12), s.RoutesSplit[i], outbound, outTimes));
                    //StopDetailModel.routeList[0].RouteName = title.Substring(12);
                    //StopDetailModel.routeList[0].times = "fwefewf";
                }
                else
                {
                    node = node.NextNode;
                    el = node as XElement;

                    el = el.Element("direction");
                    inbound = el.Attribute("title").ToString();

                    int j = 0;
                    while (j < 1)
                    {
                        el = el.Element("prediction");
                        time = el.Attribute("minutes").ToString();

                        inTimes[i] = time;
                        j++;
                    }

                    outDir = true;
                    el = node.NextNode as XElement;
                    StopDetailModel.routeList[0].GetIn(inbound, inTimes);
                    counter++;
                    i++;
                    //StopDetailModel.routeList.Add(new RouteData(title.Substring(12), s.RoutesSplit[i], outbound, outTimes, inbound, inTimes));
                    //StopDetailModel.routeList[0].RouteName = title.Substring(12);
                    //StopDetailModel.routeList[0].times = "fwefewf";
                }
                
            }  
        }
    }
}
