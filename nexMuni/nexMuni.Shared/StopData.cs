using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni
{
    public class StopData
    {
        public string Name { get; set; }
        public string [] RoutesSplit { get; set; }
        public string Routes { get; set; }

        public StopData() { }

        public StopData(string stopName, string routes)
        {
            Name = stopName;
            this.RoutesSplit = routes.Split(',');
            this.Routes = routes;
            //this.Routes = routes;
        }
    }
}
