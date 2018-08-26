using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CoreLocation;
using Foundation;
using MapKit;
using NextBus.Models;
using UIKit;

namespace NexMuni.iOS
{
    public class VehicleMapAnnotation : MKAnnotation
    {
        CLLocationCoordinate2D coordinate;
        public override CLLocationCoordinate2D Coordinate => coordinate;

        public Vehicle Vehicle { get; private set; }
        public VehicleMapAnnotation(Vehicle v, CLLocationCoordinate2D coord)
        {
            Vehicle = v;
            coordinate = coord;
        }
    }
}