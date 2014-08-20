using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;

namespace nexMuni
{
    class ErrorHandler
    {
        public static async void NetworkError(string message)
        {
            var messageBox = new MessageDialog(message);
            await messageBox.ShowAsync();
        }
    }
}
