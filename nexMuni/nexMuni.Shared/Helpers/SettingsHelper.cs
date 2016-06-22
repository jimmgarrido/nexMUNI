using System;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace nexMuni.Helpers
{
    public class SettingsHelper
    {
        //TODO: Should load ALL settings at launch. If new setting is set, then modify class property
        public static int NearbyCount { get; set; }
        public static int LaunchPivotIndex { get; set; }
        public static int TransparentTile { get; set; }
        public static DateTime RefreshedDate { get; set; }

        public static void LoadSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var roamingSettings = ApplicationData.Current.RoamingSettings;

            //Set default settings for first install
            if (roamingSettings.Values["NearbyCount"] == null) 
            {
                roamingSettings.Values["NearbyCount"] = 15;
            }

            if (roamingSettings.Values["LaunchPivot"] == null)
            {
                roamingSettings.Values["LaunchPivot"] = "nearby";
            }

            if (roamingSettings.Values["TransparentTile"] == null)
            {
                roamingSettings.Values["TransparentTile"] = 1;
            }

            if (localSettings.Values["DatabaseRefreshed"] == null)
            {
                localSettings.Values["DatabaseRefreshed"] = DateTime.Today;
            }


            //Load settings
            NearbyCount = (int) roamingSettings.Values["NearbyCount"];
            TransparentTile = (int)roamingSettings.Values["TransparentTile"];

            if ((string)roamingSettings.Values["LaunchPivot"] == "favorites")
                LaunchPivotIndex = 1;
            else
                LaunchPivotIndex = 0;

            RefreshedDate = (DateTime) localSettings.Values["DatabaseRefreshed"];
        }

        public static void LoadNearbySetting()
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if (settings.Values["NearbyCount"] == null)
            {
                settings.Values["NearbyCount"] = 15;
            }
            else if ((int)settings.Values["NearbyCount"] == 0)
            {
                settings.Values["NearbyCount"] = 15;
                NearbyCount = 15;
            }
            else if ((int)settings.Values["NearbyCount"] == 1)
            {
                settings.Values["NearbyCount"] = 25;
                NearbyCount = 25;
            }
            else
            {
                NearbyCount = (int) settings.Values["NearbyCount"];
            }

        }

        public static void LoadLaunchPivotSetting()
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if (settings.Values["LaunchPivot"] == null)
            {
                settings.Values["LaunchPivot"] = "nearby";
            }

            if ((string)settings.Values["LaunchPivot"] == "favorites")
            {
                LaunchPivotIndex = 1;
            }
            else
            {
                LaunchPivotIndex = 0;
            } 
        }

        public static bool GetTileSetting()
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if (settings.Values["TransparentTile"] == null)
            {
                settings.Values["TransparentTile"] = 1;
            }

            return (int) settings.Values["TransparentTile"] != 0;
        }

        public static void SetNearbySetting(int index)
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if (index == 1)
            {
                settings.Values["NearbyCount"] = 25;
                NearbyCount = 25;
            }
            else
            {
                settings.Values["NearbyCount"] = 15;
                NearbyCount = 15;
            }
        }

        public static void SetLaunchPivotSetting(int index)
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if (index == 1)
            {
                settings.Values["LaunchPivot"] = "favorites";
                LaunchPivotIndex = 1;
            }
            else
            {
                settings.Values["LaunchPivot"] = "nearby";
                LaunchPivotIndex = 0;
            }
        }

        public static void TileSwitchToggled(bool isOn)
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if(isOn)
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                settings.Values["TransparentTile"] = 1;
            }
            else
            {
                UpdateTile();
                settings.Values["TransparentTile"] = 0;
            }
        }

        private static void UpdateTile()
        {
            XmlDocument mediumTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Image);
            XmlNodeList imageAttributes = mediumTileXml.GetElementsByTagName("image");
            ((XmlElement)imageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/Solid150.png");
            ((XmlElement)imageAttributes[0]).SetAttribute("alt", "solid tile");
            ((XmlElement)mediumTileXml.GetElementsByTagName("binding").Item(0)).SetAttribute("branding", "none");


            XmlDocument smallTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare71x71Image);
            XmlNodeList smallImageAttributes = smallTileXml.GetElementsByTagName("image");
            ((XmlElement)smallImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/Solid71.png");
            ((XmlElement)smallImageAttributes[0]).SetAttribute("alt", "solid tile");

            IXmlNode node = mediumTileXml.ImportNode(smallTileXml.GetElementsByTagName("binding").Item(0), true);
            mediumTileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            TileNotification tileNotification = new TileNotification(mediumTileXml);

            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        public static void DatabaseRefreshed(DateTime today)
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values["DatabaseRefreshed"] = today;
            RefreshedDate = today;
        }
    }
}
