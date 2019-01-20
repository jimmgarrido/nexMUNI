using CoreLocation;
using Foundation;
using NextBus;
using MapKit;
using CoreGraphics;
using System;
using UIKit;
using NextBus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using NexMuni.iOS.Data;

namespace NexMuni.iOS
{
    public partial class ViewController : UIViewController
    {
        CLLocationCoordinate2D mapCenter = new CLLocationCoordinate2D(37.769031, -122.460487);
        List<Vehicle> vehicleList;
        List<Vehicle> newVehicleList;
        List<RedesignedInfoItem> redesignedList;
        UIBarButtonItem filterBtn, refreshBtn, routeBtn, flexSpace, trainsBtn;
        //List<int> redesignedIds = new List<int> { 1448, 1423, 1440, 1412, 1510, 1442, 1474, 1421, 1455, 1447, 1537, 1446, 1409 };
        NextBusClient client;
        string currentFilter, currentRoute;
        AzureService service;
        Task sync;

        public ViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            // Perform any additional setup after loading the view, typically from a nib.

            service = new AzureService();
            sync = service.InitializeAsync();

            mapView.ShowsUserLocation = true;
            mapView.CenterCoordinate = mapCenter;
            mapView.SetRegion(MKCoordinateRegion.FromDistance(mapCenter, 11500, 11500), false);
            mapView.Delegate = new MapDelegate();

            filterBtn = new UIBarButtonItem("Showing all", UIBarButtonItemStyle.Plain, ShowFilters);
            refreshBtn = new UIBarButtonItem("Refresh", UIBarButtonItemStyle.Plain, RefreshVehicles);
            routeBtn = new UIBarButtonItem("Route", UIBarButtonItemStyle.Plain, ShowRoutes);
            trainsBtn = new UIBarButtonItem("Trains", UIBarButtonItemStyle.Plain, EditTrains);
            flexSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

            SetToolbarItems(new UIBarButtonItem[] { filterBtn, flexSpace, refreshBtn}, true);

            NavigationItem.SetRightBarButtonItem(routeBtn, true);
            NavigationItem.SetLeftBarButtonItem(trainsBtn, true);

            await sync;
        }

        public override void DidReceiveMemoryWarning ()
        {
            base.DidReceiveMemoryWarning ();
            // Release any cached data, images, etc that aren't in use.
        }

        public async override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            currentFilter = "all";
            currentRoute = "N";

            await LoadRouteDataAsync();
            AddVehiclesToMap();
        }

        void AddVehiclesToMap()
        {
            CLLocationCoordinate2D coord;

            mapView.RemoveAnnotations(mapView.Annotations);
            List<Vehicle> vehicles = new List<Vehicle>();

            switch (currentFilter)
            {
                case "all":
                    vehicles = vehicleList;
                    break;
                case "new":
                    vehicles = vehicleList.FindAll(x => x.Id.ToString().StartsWith("2") && !string.IsNullOrEmpty(x.Direction));
                    break;
                case "redesigned":
                    vehicles = vehicleList.FindAll(x => redesignedList.Exists(r => r.TrainId == x.Id));
                    break;
            }

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

        void ShowRoutes(object sender, EventArgs args)
        {
            var alert = UIAlertController.Create("", "Select Route", UIAlertControllerStyle.ActionSheet);

            alert.AddAction(UIAlertAction.Create("E", UIAlertActionStyle.Default, a => ChangeRoute("E")));
            alert.AddAction(UIAlertAction.Create("F", UIAlertActionStyle.Default, a => ChangeRoute("F")));
            alert.AddAction(UIAlertAction.Create("J", UIAlertActionStyle.Default, a => ChangeRoute("J")));
            alert.AddAction(UIAlertAction.Create("K", UIAlertActionStyle.Default, a => ChangeRoute("K")));
            alert.AddAction(UIAlertAction.Create("L", UIAlertActionStyle.Default, a => ChangeRoute("L")));
            alert.AddAction(UIAlertAction.Create("M", UIAlertActionStyle.Default, a => ChangeRoute("M")));
            alert.AddAction(UIAlertAction.Create("N", UIAlertActionStyle.Default, a => ChangeRoute("N")));
            alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            PresentViewController(alert, true, null);
        }

        void EditTrains(object sender, EventArgs args)
        {
            var controller = Storyboard.InstantiateViewController("TrainsController") as TrainsViewController;
            controller.Parent = NavigationController;
            controller.Items = redesignedList;

            if (controller != null)
            {
                var nav = new UINavigationController(controller);
                PresentViewController(nav, true, null);

            }
        }

        async void ChangeRoute(string route)
        {
            currentRoute = route;
            await LoadRouteDataAsync();
            AddVehiclesToMap();
        }

        async void RefreshVehicles(object sender, EventArgs args)
        {
            vehicleList = await client.GetVehicleLocations(currentRoute);

            //newVehicleList = vehicleList.FindAll(x => x.Id.ToString().StartsWith("2") && !string.IsNullOrEmpty(x.Direction));
            AddVehiclesToMap();
            //switch (currentFilter)
            //{
            //    case "all":
            //        AddVehiclesToMap(vehicleList);
            //        break;
            //    case "new":
            //        AddVehiclesToMap(newVehicleList);
            //        break;
            //    case "redesigned":
            //        AddVehiclesToMap(redesignedList);
            //        break;
            //}
        }

        void FilterVehicles(string filter)
        {
            currentFilter = filter;
            filterBtn.Title = $"Showing {currentFilter}";

            AddVehiclesToMap();

            //switch (currentFilter)
            //{
            //    case "all":
            //        AddVehiclesToMap("a);
            //        break;
            //    case "new":
            //        AddVehiclesToMap(newVehicleList);
            //        break;
            //    case "redesigned":
            //        AddVehiclesToMap(redesignedList);
            //        break;
            //}
        }

        async Task LoadRouteDataAsync()
        {
            client = new NextBusClient("sf-muni");
            vehicleList= await client.GetVehicleLocations(currentRoute);
            var data = await client.GetRouteConfig(currentRoute);

            Title = data.Title;

            newVehicleList = vehicleList.FindAll(x => x.Id.ToString().StartsWith("2") && !string.IsNullOrEmpty(x.Direction));
            //redesignedList = vehicleList.FindAll(x => redesignedIds.Contains(x.Id));
            redesignedList = await service.GetItemsAsync();
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
                        view.Image = UIImage.FromBundle("Inbound");
                    }

                    view.Transform = CGAffineTransform.MakeIdentity();
                    view.Transform = CGAffineTransform.MakeRotation((nfloat)(currentAnnotation.Vehicle.Heading * (Math.PI/180)));

                    view.CanShowCallout = true;
                    return view;
                }
                else
                {
                    view = mapView.DequeueReusableAnnotation(outbound);

                    if (view == null)
                    {
                        view = new MKAnnotationView(annotation, outbound);
                        view.Image = UIImage.FromBundle("Outbound");
                    }

                    view.Transform = CGAffineTransform.MakeIdentity();
                    view.Transform = CGAffineTransform.MakeRotation((nfloat)(currentAnnotation.Vehicle.Heading * (Math.PI / 180)));

                    view.CanShowCallout = true;
                    return view;
                }
            }

            view = mapView.DequeueReusableAnnotation(unknown);

            if (view == null)
            {
                view = new MKAnnotationView(annotation, unknown);
                view.Image = UIImage.FromBundle("Marker");
            }

            view.CanShowCallout = true;
            return view;
        }
    }

    class MyVehicle : Vehicle
    {
        public string Kind { get; set; }
    }
}