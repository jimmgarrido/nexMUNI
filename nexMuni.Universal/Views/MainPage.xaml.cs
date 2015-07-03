using nexMuni.Helpers;
using nexMuni.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace nexMuni.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel mainVm;
        private bool alreadyLoaded;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!alreadyLoaded)
            {
                //MainPivot.SelectionChanged += PivotItemChanged;
                await DatabaseHelper.CheckDatabasesAsync();

                mainVm = new MainViewModel();
                //searchVm = new SearchViewModel();
                //searchVm.UpdateLocation += LocationUpdated;

                NearbyPivot.DataContext = mainVm;
                //FavoritesPivot.DataContext = mainVm;
                //SearchPivot.DataContext = searchVm;

                //RoutesFlyout.ItemsPicked += RouteSelected;
                //DirBox.SelectionChanged += DirectionSelected;
                //StopsFlyout.ItemsPicked += StopSelected;

                //MapControl.SetNormalizedAnchorPoint(LocationIcon, new Point(0.5, 0.5));
                //MapControl.SetNormalizedAnchorPoint(StopIcon, new Point(0.5, 1.0));

                alreadyLoaded = true;
            }
        }

        private void StopClicked(object sender, ItemClickEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
