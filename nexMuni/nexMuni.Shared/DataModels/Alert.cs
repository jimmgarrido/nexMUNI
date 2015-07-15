using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni.DataModels
{
    public class Alert
    {
        public string AffectedRoutes { get; set; }
        public string Message { get; set; }

        public Alert(string route, string text)
        {
            AffectedRoutes = "Routes: " + route;           
            Message = text;
        }

        public void AddRoute(string route)
        {
            if(!AffectedRoutes.Contains(route))
                AffectedRoutes = AffectedRoutes + ", " + route;
        }
    }
}
