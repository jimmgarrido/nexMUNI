using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni.DataModels
{
    public class RouteDirection
    {
        public string Title { get; set; }
        public string Times { get; set; }

        public RouteDirection(string title)
        {
            Title = title;
        }
    }
}
