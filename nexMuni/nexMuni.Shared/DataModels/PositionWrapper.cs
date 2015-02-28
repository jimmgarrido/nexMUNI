using System;
using System.Collections.Generic;
using System.Text;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace nexMuni.DataModels
{
    public class PositionWrapper
    {
        public Geopoint Position { get; set; }
        public static Point AnchorPoint { get; set; }

        static PositionWrapper()
        {
            AnchorPoint = new Point(0.32, 0.78);
        }

        public PositionWrapper(Geopoint p)
        {
            Position = p;
            //AnchorPoint = new Point(0.32, 0.78);
        }
    }
}
