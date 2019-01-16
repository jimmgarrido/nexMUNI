using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace NexMuni.iOS
{
    public partial class TrainsViewController : UITableViewController
    {
        public UINavigationController Parent { get; set; }

        public TrainsViewController (IntPtr handle) : base (handle)
        {
            TableView.Source = new TrainsSource();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var closeBtn = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, (s, e) => Parent.DismissViewController(true, null));
            NavigationItem.SetLeftBarButtonItem(closeBtn, true);
        }
    }

    class TrainsSource : UITableViewSource
    {
        List<string> items;

        public TrainsSource()
        {
            items = new List<string>() { "item", "Item2", "item3" };
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = new UITableViewCell(UITableViewCellStyle.Default, "cell");
            cell.TextLabel.Text = items[indexPath.Row];

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return items.Count;
        }
    }
}