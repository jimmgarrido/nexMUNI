using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni
{
    public class RouteData
    {
        public string RouteNum { get; set; }
        public string RouteName { get; set; }
        public string outTimes { get; set; }
        public string inTimes { get; set; }
        public string outTitle { get; set; }
        public string inTitle { get; set; }

        public RouteData() { }

        public RouteData(string name, string num, string _outTitle, string [] _outTimes)
        {
            RouteName = name;
            RouteNum = num;
            outTitle = _outTitle;
            outTimes = String.Join(", ",_outTimes) + " mins";
        }

        public void GetIn(string _inTitle, string [] _inTimes)
        {
            inTitle = _inTitle;
            inTimes = String.Join(", ",_inTimes) + " mins";
        }
    }
}
