using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace nexMuni.Helpers
{
    public class UIHelper
    {
        public static async Task ShowStatusBar(string text)
        {
#if WINDOWS_PHONE_APP
            var statusBar = StatusBar.GetForCurrentView();
            await statusBar.ProgressIndicator.ShowAsync();
            statusBar.ProgressIndicator.Text = text;
            statusBar.ProgressIndicator.ProgressValue = null;
#else
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.ProgressIndicator.ShowAsync();
                statusBar.ProgressIndicator.Text = text;
                statusBar.ProgressIndicator.ProgressValue = null;
            }
#endif
        }

        public static async Task HideStatusBar()
        {
#if WINDOWS_PHONE_APP
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.ProgressValue = 0;
            await statusBar.ProgressIndicator.HideAsync();
#else
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.ProgressValue = 0;
                await statusBar.ProgressIndicator.HideAsync();
            }
#endif
        }
    }
}
