using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;

namespace nexMuni.Helpers
{
    class SettingsHelper
    {
        public static int GetNearbySetting()
        {
            var settings = ApplicationData.Current.RoamingSettings;
            //settings.Values.Remove("NearbyCount");
            if (settings.Values["NearbyCount"] == null)
            {
                settings.Values["NearbyCount"] = 15;
            }

            return (int)settings.Values["NearbyCount"];
        }

        public static void SetNearbySetting(int index)
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if (index == 1)
            {
                settings.Values["NearbyCount"] = 25;
            }
            else
            {
                settings.Values["NearbyCount"] = 15;
            }
            
        }
    }
}
