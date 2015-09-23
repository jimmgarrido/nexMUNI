using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace nexMuni.Helpers
{
    public class SettingsHelper
    {
        public static int GetNearbySetting()
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if (settings.Values["NearbyCount"] == null)
            {
                settings.Values["NearbyCount"] = 15;
            }

            if ((int)settings.Values["NearbyCount"] == 25) return 1;
            else return 0;
        }

        public static int GetLaunchPivotSetting()
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if (settings.Values["LaunchPivot"] == null)
            {
                settings.Values["LaunchPivot"] = "nearby";
            }
            var test = settings.Values["LaunchPivot"];
            if ((string)settings.Values["LaunchPivot"] == "favorites") return 1;
            else return 0; 
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
            }
            else
            {
                settings.Values["NearbyCount"] = 15;
            }
        }

        public static void SetLaunchPivotSetting(int index)
        {
            var settings = ApplicationData.Current.RoamingSettings;
            if (index == 1)
            {
                settings.Values["LaunchPivot"] = "favorites";
            }
            else
            {
                settings.Values["LaunchPivot"] = "nearby";
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
    }
}
