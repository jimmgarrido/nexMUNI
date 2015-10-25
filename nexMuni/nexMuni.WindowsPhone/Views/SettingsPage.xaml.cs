using nexMuni.Common;
using nexMuni.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace nexMuni.Views
{
    public sealed partial class SettingsPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public SettingsPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            CountBox.SelectionChanged += ChangeNearbyCount;
            PivotBox.SelectionChanged += ChangePivotSetting;
            TileSwitch.Toggled += TileSwitchToggled;

            if(SettingsHelper.nearbyCount == 15)
            {
                CountBox.SelectedIndex = 0;
            }
            else
            {
                CountBox.SelectedIndex = 1;
            }


            PivotBox.SelectedIndex = SettingsHelper.launchPivot;

            if(SettingsHelper.GetTileSetting())
            {
                TileSwitch.IsOn = true;
            }
            else
            {
                TileSwitch.IsOn = false;
            }
        }

        private void TileSwitchToggled(object sender, RoutedEventArgs e)
        {
            SettingsHelper.TileSwitchToggled(((ToggleSwitch)sender).IsOn);
        }

        private void ChangeNearbyCount(object sender, SelectionChangedEventArgs e)
        {
            SettingsHelper.SetNearbySetting(((ComboBox) sender).SelectedIndex);
        }

        private void ChangePivotSetting(object sender, SelectionChangedEventArgs e)
        {
            SettingsHelper.SetLaunchPivotSetting(((ComboBox) sender).SelectedIndex);
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }


        #endregion
    }
}
