using Foundation;
using NexMuni.iOS.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace NexMuni.iOS
{
    public partial class TrainsViewController : UITableViewController
    {
        public UINavigationController Parent { get; set; }
        public TrainsSource Source { get; set; }

        public TrainsViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Source = Source;

            var closeBtn = new UIBarButtonItem("Close", UIBarButtonItemStyle.Plain, (s, e) => Parent.DismissViewController(true, null));
            var addBtn = new UIBarButtonItem("Add", UIBarButtonItemStyle.Plain, ShowAddPopup);

            NavigationItem.SetLeftBarButtonItem(closeBtn, true);
            NavigationItem.SetRightBarButtonItem(addBtn, true);
        }

        void ShowAddPopup(object sender, EventArgs args)
        {
            var textField = new UITextField();
            var alert = UIAlertController.Create("Add Train Info", "Redesigned train id:", UIAlertControllerStyle.Alert);
            var trainId = string.Empty;

            alert.AddAction(UIAlertAction.Create("Add", UIAlertActionStyle.Default, async a => await AddTrainItem(alert.TextFields[0].Text)));
            alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            alert.AddTextField((t) => { } );

            PresentViewController(alert, true, null);
        }

        async Task AddTrainItem(string id)
        {
            var item = new RedesignedInfoItem { TrainId = int.Parse(id) };
            await AzureService.Instance.AddTrainInfo(item);
            Source.redesignedInfoItems = await AzureService.Instance.GetItemsAsync();
            TableView.ReloadData();
            await AzureService.Instance.SyncService();
        }
    }

    public class TrainsSource : UITableViewSource
    {
        public List<RedesignedInfoItem> redesignedInfoItems;

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