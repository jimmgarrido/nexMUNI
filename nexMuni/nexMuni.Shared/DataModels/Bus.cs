using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni.DataModels
{
    public class Bus
    {
        public int busId;
        public int busHeading;
        public double latitude;
        public double longitude;

        public Bus(string id, string heading, string lat, string lon)
        {
            busId = int.Parse(id);
            busHeading = int.Parse(heading);
            latitude = double.Parse(lat);
            longitude = double.Parse(lon);
        }
    }
}
