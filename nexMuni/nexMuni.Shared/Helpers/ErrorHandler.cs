using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace nexMuni
{
    public class ErrorHandler
    {
        public static async Task NetworkError(string message)
        {
            var messageBox = new MessageDialog(message);
            await messageBox.ShowAsync();
        }
    }
}
