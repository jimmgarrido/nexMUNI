using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace nexMuni.DataModels
{
    public class Route
    {
        private string predictions1 = String.Empty;
        private string predictions2 = String.Empty;

        public string RouteName { get; private set; }
        public string RouteNumber { get; private set; }
        public List<RouteDirection> Directions {get; set;}
        public string Dir1 { get; set; }
        public string Dir2 { get; set; }

        public string Times1 { get; set; }
        public string Times2 { get; set; }

        private Route() { }

        public Route(string name, string num)
        {
            RouteName = name;
            RouteNumber = num;
            Directions = new List<RouteDirection>();
        }

        public Route(string name, string num, string dir, string [] _times)
        {
            int i = 0;
            RouteName = name;
            RouteNumber = num;
            Dir1 = dir;
            if (Dir1.Contains("via Downtown")) Dir1 = Dir1.Replace("via Downtown", "");
            if (Dir1.Contains("&amp;")) Dir1 = Dir1.Replace("&amp;", "&");
            if (Dir1.Contains("Daly City BART Station")) Dir1 = Dir1.Replace("( Daly City BART Station 4pm-7pm)", "BART");

            while (i < _times.Length && _times[i] != null)
            {
                if (i == 0)
                {
                    if (_times[0] == "0") Times1 = "<1";
                    else Times1 = _times[0];
                    
                    i++;
                }
                else if (_times[i] != null)
                {
                    Times1 = Times1 + ", " + _times[i];
                    i++;
                }
            }

            Times1 = Times1 + " mins";
        }

        public void AddDir2(string _inTitle, string [] _times)
        {
            int i= 0;
            Dir2 = _inTitle;
            if (Dir2.Contains("via Downtown")) Dir2 = Dir2.Replace("via Downtown", "");
            if (Dir2.Contains("&amp;")) Dir2 = Dir2.Replace("&amp;", "&");
            if (Dir2.Contains("Daly City BART Station")) Dir2 = Dir2.Replace("( Daly City BART Station 4pm-7pm)", "BART");

            while (i < _times.Length && _times[i] != null)
            {
                if (i == 0)
                {
                    if (_times[0] == "0") Times2 = "<1";
                    else Times2 = _times[0];
                    i++;
                }
                else if (_times[i] != null)
                {
                    Times2 = Times2 + ", " + _times[i];
                    i++;
                }
            }

            Times2 = Times2 + " mins"; 
        }
    }
}
