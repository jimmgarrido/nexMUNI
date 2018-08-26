using CoreLocation;
using Foundation;
using NextBus;
using MapKit;
using CoreGraphics;
using System;
using UIKit;
using NextBus.Models;
using System.Collections.Generic;

namespace NexMuni.iOS
{
    public partial class ViewController : UIViewController
    {
        CLLocationCoordinate2D mapCenter = new CLLocationCoordinate2D(37.769031, -122.460487);
        List<Vehicle> vehicleList;
        List<Vehicle> newVehicleList;
        bool isToggled = false;

        public ViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            // Perform any additional setup after loading the view, typically from a nib.

            mapView.ShowsUserLocation = true;
            mapView.CenterCoordinate = mapCenter;
            mapView.SetRegion(MKCoordinateRegion.FromDistance(mapCenter, 11500, 11500), false);
            mapView.Delegate = new MapDelegate();

            SetToolbarItems(new UIBarButtonItem[] { new UIBarButtonItem("Filter", UIBarButtonItemStyle.Plain, ShowFilters) }, true);
        }

        public override void DidReceiveMemoryWarning ()
        {
            base.DidReceiveMemoryWarning ();
            // Release any cached data, images, etc that aren't in use.
        }

        public async override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            var client = new NextBusClient("sf-muni");
            vehicleList = await client.GetVehicleLocations("N");

            newVehicleList = vehicleList.FindAll(x => x.Id.ToString().StartsWith("2") && !string.IsNullOrEmpty(x.Direction));

            AddVehiclesToMap(vehicleList);
        }

        void AddVehiclesToMap(List<Vehicle> vehicles)
        {
            CLLocationCoordinate2D coord;

            mapView.RemoveAnnotations(mapView.Annotations);

            foreach (var v in vehicles)
            {
                coord = new CLLocationCoordinate2D(v.Latitude, v.Longitude);

                mapView.AddAnnotation(new VehicleMapAnnotation(v, coord));
            }
        }

        void ShowFilters (object sender, EventArgs args)
        {
            var alert = UIAlertController.Create("Filter Trains", "Only show:", UIAlertControllerStyle.ActionSheet);

            alert.AddAction(UIAlertAction.Create("All", UIAlertActionStyle.Default, a => FilterVehicles("all")));
            alert.AddAction(UIAlertAction.Create("New", UIAlertActionStyle.Default, a => FilterVehicles("new")));
            alert.AddAction(UIAlertAction.Create("Redesigned", UIAlertActionStyle.Default, a => FilterVehicles("redesigned")));
            alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            PresentViewController(alert, true, null);
        }

        void FilterVehicles(string filter)
        {
            switch (filter)
            {
                case "all":
                    AddVehiclesToMap(vehicleList);
                    break;
                case "new":
                    AddVehiclesToMap(newVehicleList);
                    break;
            }
        }
    }

    public class MapDelegate : MKMapViewDelegate
    {
        string inbound = "inbound";
        string outbound = "outbound";
        string unknown = "unknown";

        public override MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
        {
            if (annotation is MKUserLocation)
                return null;

            var currentAnnotation = annotation as VehicleMapAnnotation;
            MKAnnotationView view;

            if(currentAnnotation.Vehicle.Direction != null)
            {
                if (currentAnnotation.Vehicle.Direction.Contains("I"))
                {
                    view = mapView.DequeueReusableAnnotation(inbound);

                    if (view == null)
                    {
                        view = new MKAnnotationView(annotation, inbound);
                        view.Image = UIImage.FromFile("inbound.png");
                    }

                    view.Transform = CGAffineTransform.MakeIdentity();
                    view.Transform = CGAffineTransform.MakeRotation(currentAnnotation.Vehicle.Heading);
                    return view;
                }
                else
                {
                    view = mapView.DequeueReusableAnnotation(outbound);

                    if (view == null)
                    {
                        view = new MKAnnotationView(annotation, inbound);
                        view.Image = UIImage.FromFile("outbound.png");
                    }

                    view.Transform = CGAffineTransform.MakeIdentity();
                    view.Transform = CGAffineTransform.MakeRotation(currentAnnotation.Vehicle.Heading);
                    return view;
                }
            }

            view = mapView.DequeueReusableAnnotation(unknown);

            if (view == null)
            {
                view = new MKAnnotationView(annotation, unknown);
                view.Image = UIImage.FromFile("marker.png");
            }

            return view;
        }
    }
}