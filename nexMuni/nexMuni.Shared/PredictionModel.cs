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
            GetXML(StopDetailModel.url, list);
        }

        private static async void GetXML(string url, ObservableCollection<RouteData> stops)
        {
            HttpClient client = new HttpClient();
            XDocument xmlDoc = new XDocument();
            Stream xmlStream;

            xmlStream = await client.GetStreamAsync(url);
            xmlDoc = XDocument.Load(xmlStream);

            GetPredictions(xmlDoc, stops);
        }

        private static void GetPredictions(XDocument doc, ObservableCollection<RouteData> s)
        {
            XNode node = doc.Root.FirstNode;

            XElement el = node as XElement;
            string routeTag = el.Attribute("routeTag").ToString();

            if(routeTag.Equals("routeTag=\"" + s[0].RouteNum.Substring(1) + "\""))
            {
                string title = el.Attribute("routeTitle").ToString();
                StopDetailModel.routeList[0].RouteName = title.Substring(12);
                StopDetailModel.routeList[0].times = "fwefewf";
            }
            
        }
    }
}
