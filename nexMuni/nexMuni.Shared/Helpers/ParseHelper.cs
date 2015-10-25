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

        public static List<string> ParseDirections(XDocument document)
        {
            var directions = new List<string>();
            var elements = document.Element("body").Element("route").Elements("direction");

            foreach (XElement el in elements)
            {
                //Add direction title
                directions.Add(el.Attribute("title").Value);

                //IEnumerable<XElement> tagElements;
               
            }
            return directions;
        }

        public static List<Stop> ParseStops(XDocument document)
        {
            var stopsList = new List<Stop>();
            var elements = document.Element("body").Element("route").Elements("stop");

            //Add all route's stops to a collection
            foreach (XElement el in elements)
            {
                stopsList.Add(new Stop(el.Attribute("title").Value,
                                              el.Attribute("stopId").Value,
                                              "",
                                              el.Attribute("tag").Value,
                                              double.Parse(el.Attribute("lon").Value),
                                              double.Parse(el.Attribute("lat").Value)));
            }

            return stopsList;
        }

        public static void ParseStopTags(XDocument document, List<string> inbound, List<string> outbound)
        {
            var rootElements = document.Element("body").Element("route").Elements("direction");

            foreach(var direction in rootElements)
            {
                if (direction.Attribute("name").Value == "Inbound")
                {
                    //Get all stop elements under direction element
                    var tagElements = direction.Elements("stop");

                    if (inbound.Count != 0) inbound.Clear();
                    //Add tags for direction to a collection
                    foreach (XElement stop in tagElements)
                    {
                        inbound.Add(stop.Attribute("tag").Value);
                    }
                }
                else if (direction.Attribute("name").Value == "Outbound")
                {
                    //Get all stop elements under direction element
                    var tagElements = direction.Elements("stop");

                    if (outbound.Count != 0) outbound.Clear();
                    //Add tags for direction to a collection
                    foreach (XElement stop in tagElements)
                    {
                        outbound.Add(stop.Attribute("tag").Value);
                    }
                }
            }
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
