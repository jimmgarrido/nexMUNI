using Foundation;
using NexMuni.iOS.Data;
using System;
using System.Collections.Generic;
using UIKit;

namespace NexMuni.iOS
{
    public partial class TrainsViewController : UITableViewController
    {
        public UINavigationController Parent { get; set; }
        public List<RedesignedInfoItem> Items { get; set; }

        public TrainsViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Source = new TrainsSource(Items);

            var closeBtn = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, (s, e) => Parent.DismissViewController(true, null));
            NavigationItem.SetLeftBarButtonItem(closeBtn, true);
        }
    }

    class TrainsSource : UITableViewSource
    {
        List<RedesignedInfoItem> redesignedInfoItems;

        public TrainsSource(List<RedesignedInfoItem> items)
        {
            redesignedInfoItems = items;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Default, "cell");
            cell.TextLabel.Text = redesignedInfoItems[indexPath.Row].TrainId.ToString();

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return redesignedInfoItems.Count;
        }
    }
}