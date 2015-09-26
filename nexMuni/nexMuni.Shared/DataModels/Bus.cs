using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

namespace nexMuni.DataModels
{
    public class Bus
    {
        public int busId;
        public int busHeading;
        public string direction;
        public double latitude;
        public double longitude;

        public Bus(string id, string heading, string lat, string lon, string dir)
        {
            var telemetry = new TelemetryClient();
            try {
                busId = int.Parse(id);
                busHeading = int.Parse(heading);
                latitude = double.Parse(lat);
                longitude = double.Parse(lon);
            }
            catch(Exception ex)
            {
                telemetry.TrackTrace(String.Format("id:{0}, heading:{1}", id, heading));
            }

            if (dir.Contains("I")) direction = "inbound";
            else if(dir.Contains("O")) direction = "outbound";
        }
    }
}
