using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni.DataModels
{
    public class Alert
    {
        public string AffectedRoute { get; set; }
        public string Message { get; set; }

        public Alert(string route, string text)
        {
            AffectedRoute = route;
            Message = text;
        }
    }
}
