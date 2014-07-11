using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni
{
    public class RouteData
    {
        public string RouteNum { get; set; }
        public string RouteName { get; set; }
        public string times { get; set; }

        public RouteData() { }

        public RouteData(string num)
        {
            RouteNum = num;
        }
    }
}
