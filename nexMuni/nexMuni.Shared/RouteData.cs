using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni
{
    public class RouteData
    {
        public string RouteNum { get; set; }
        public string RouteName { get; set; }
        public string Dir1 { get; set; }
        public string Times1 { get; set; }
        public string Dir2 { get; set; }
        public string Times2 { get; set; }

        public RouteData() { }

        public RouteData(string name, string num, string dir, string [] _times)
        {
            int i = 0;
            RouteName = name;
            RouteNum = num;
            Dir1 = dir;
            if (Dir1.Contains("via Downtown")) Dir1 = Dir1.Replace("via Downtown", "");
            if (Dir1.Contains("&amp;")) Dir1 = Dir1.Replace("&amp;", "&");

            if (_times.Length > 1)
            {
                while (i < _times.Length - 1)
                {
                    Times1 = Times1 + _times[i] + ", ";
                    i++;
                }
                Times1 = Times1 + _times[i] + " mins";
            }
            else Times1 = _times + " mins";
        }

        public void AddDir2(string _inTitle, string [] _times)
        {
            int i= 0;
            Dir2 = _inTitle;
            if (Dir2.Contains("via Downtown")) Dir2 = Dir2.Replace("via Downtown", "");
            if (Dir2.Contains("&amp;")) Dir2 = Dir2.Replace("&amp;", "&");

            if (_times.Length > 1)
            {
                while (i < _times.Length - 1)
                {
                    Times2 = Times2 + _times[i] + ", ";
                    i++;
                }
                Times2 = Times2 + _times[i] + " mins";
            }
            else Times2 = _times[0] + " mins";  
        }
    }
}
