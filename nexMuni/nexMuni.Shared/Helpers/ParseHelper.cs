using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using nexMuni.DataModels;

namespace nexMuni.Helpers
{
    public class ParseHelper
    {
        public static async Task<List<Route>> ParsePredictionsAsync(XDocument document)
        {
            var routes = new List<Route>();
            string routeNum;
            IEnumerable<XElement> rootElements;

            //If there was an error getting the xml, return an empty list
            if (document.Root == null) return routes;

            rootElements = document.Element("body").Elements("predictions");
            foreach(XElement predictionElement in rootElements)
            {
                string routeTitle = ParseTitle(predictionElement);
                routeNum = ParseRouteNum(predictionElement);

                //Check to see if the route has already been added to the collection
                if (!routes.Any(r => r.RouteNumber == routeNum))
                {
                    Route newRoute = new Route(routeTitle, routeNum);

                    var subElement = predictionElement.Element("direction");
                    if (subElement != null)
                    {
                        var dirTitle = subElement.Attribute("title").Value;
                        var times = ParseTimes(subElement);
                        newRoute.Directions.Add(new RouteDirection(dirTitle, times));
                        routes.Add(newRoute);
                    }
                }
                else
                {
                    Route tempRoute = routes.Find(r => r.RouteNumber == routeNum);

                    var subElement = predictionElement.Element("direction");
                    if (subElement != null)
                    {
                        var dirTitle = subElement.Attribute("title").Value;
                        var times = ParseTimes(subElement);
                        tempRoute.Directions.Add(new RouteDirection(dirTitle, times));
                    }

                }   
            }
            return routes;
        }

        public static async Task<string> ParseSearchTimesAsync(XDocument document)
        {
            var element = document.Element("body").Element("predictions").Element("direction");
            return element != null ? ParseTimes(element) : "No times found";
        }

        public static async Task<List<Alert>> ParseAlerts(XDocument document)
        {
            if (document.Root == null) return new List<Alert>();

            var alerts = new List<Alert>();
            var rootElements = document.Element("body").Elements("predictions").Where(
                e => e.Attributes().All(a => a.Name != "dirTitleBecauseNoPredictions"));

            foreach (XElement predictionElement in rootElements)
            {
                var messageElements = predictionElement.Elements("message");

                foreach (XElement messageElement in messageElements)
                {
                    if(!messageElement.Attribute("priority").Value.Equals("Low"))
                    {
                        var route = predictionElement.Attribute("routeTag").Value;
                        var message = messageElement.Attribute("text").Value.Replace("\n", " ");

                        if (alerts.Any(x => x.Message == message))
                        {
                            alerts.Find(b => b.Message == message).AddRoute(route);
                        }
                        else
                        {
                            alerts.Add(new Alert(route, message));
                        }
                    }
                }
            }

            return alerts;
        }

        private static string ParseTimes(XElement element)
        {
            int maxTimes;
            var builder = new StringBuilder();
            var predictionElements = element.Elements("prediction");

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
            return builder.ToString();
        }

        private static string ParseTitle(XElement element)
        {
            string fullTitle = element.Attribute("routeTitle").Value;
            if (fullTitle.Contains('-'))
            {
                int index = fullTitle.IndexOf('-');
                return fullTitle.Substring(index + 1, fullTitle.Length - (index + 1));
            }
            else
            {
                int index = fullTitle.IndexOf('"');
                return fullTitle.Substring(index + 1, (fullTitle.Length - (index + 2)));
            }
        }

        private static string ParseRouteNum(XElement element)
        {
            string fullTitle = element.Attribute("routeTitle").Value;
            if (fullTitle.Contains('-'))
            {
                return fullTitle.Substring(0, fullTitle.IndexOf('-'));
            }
            else
            {
                return element.Attribute("routeTag").Value;
            }
        }
    }
}
