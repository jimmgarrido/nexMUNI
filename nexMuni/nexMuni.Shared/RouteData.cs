using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni
{
    public class RouteData
    {
        public string RouteNum { get; set; }
        public string RouteName { get; set; }

        public RouteData() { }

        public RouteData(string num)
        {
            RouteNum = num;
            //this.Routes = routes.Split(',');
            //this.Routes = routes;
        }
    }
}
