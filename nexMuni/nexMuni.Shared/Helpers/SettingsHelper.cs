using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace nexMuni.Helpers
{
    class SettingsHelper
    {
        public static int GetNearbySetting()
        {
            var settings = ApplicationData.Current.RoamingSettings;

            if (settings.Values["NearbyCount"] == null)
            {
                settings.Values["NearbyCount"] = 15;
            }

            return (int) settings.Values["NearbyCount"];
        }

        public static bool GetTileSetting()
        {
            var settings = ApplicationData.Current.RoamingSettings;

            if (settings.Values["TransparentTile"] == null)
            {
                settings.Values["TransparentTile"] = 1;
            }

            if ((int) settings.Values["TransparentTile"] == 0) return false;
            else return true;
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

        public static void TileSwitchToggled(bool isOn)
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if(isOn)
            {
                Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                settings.Values["TransparentTile"] = 1;
            }
            else
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
                settings.Values["TransparentTile"] = 0;
            }
        }
    }
}
