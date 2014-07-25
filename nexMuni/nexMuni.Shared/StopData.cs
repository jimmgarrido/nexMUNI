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
        public string [] TagsSplit { get; set; }
        public string Distance { get; set; }
        public string Tags { get; set; }

        public StopData() { }

        public StopData(string stopName, string routes)
        {
            Name = stopName;
            this.RoutesSplit = routes.Split(',');
            this.Routes = routes;
        }

        public StopData(string stopName, string routes, string _tags, double d)
        {
            Name = stopName;
            this.RoutesSplit = routes.Split(',');
            RoutesSplit[0] = " " + RoutesSplit[0];
            this.Tags = _tags;
            this.TagsSplit = _tags.Split(',');
            this.Routes = routes;
            Distance = d.ToString("F") + " mi";
        }

        public StopData(string stopName, string routes, string _tags, string d)
        {
            Name = stopName;
            this.Tags = _tags;
            this.Routes = routes;
            Distance = d;
        }
    }
}
