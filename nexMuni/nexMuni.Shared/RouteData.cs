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
            if (outTitle.Contains("via Downtown")) outTitle = outTitle.Replace("via Downtown", "");
            if (outTitle.Contains("&amp;")) outTitle = outTitle.Replace("&amp;", "&");
            outTimes = String.Join(", ",_outTimes) + " mins";
        }

        public void GetIn(string _inTitle, string [] _inTimes)
        {
            inTitle = _inTitle;
            if (inTitle.Contains("via Downtown")) inTitle = inTitle.Replace("via Downtown", "");
            if (inTitle.Contains("&amp;")) inTitle = inTitle.Replace("&amp;", "&");
            inTimes = String.Join(", ",_inTimes) + " mins";
        }
    }
}
