using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using SQLite;
using Windows.Devices.Geolocation;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace nexMuni
{
    class DatabaseHelper
    {
        protected static string path = string.Empty;

        public static List<BusStop> QueryDatabase(double[][] b, Geopoint l, double d, int c)
        {
            var db = new SQLiteConnection("db/muni.sqlite");
            string query = "SELECT * FROM BusStops WHERE Longitude BETWEEN " + b[3][1] + " AND " + b[1][1] + " AND Latitude BETWEEN " + b[2][0] + " AND " + b[0][0];
            List<BusStop> r = db.Query<BusStop>(query);

            if ((r.Count == 0 || r.Count < 15))
            {
                c++;
                LocationHelper.FindNearby(l, d += 0.50, c);

            }
            else db.Close();

            return r;
        }

        public static List<Routes> QueryForRoutes()
        {
            var db = new SQLiteConnection("db/muni.sqlite");
            string query = "SELECT * FROM Routes";

            List<Routes> list = db.Query<Routes>(query);
            db.Close();
            return list;
        }

        public static async void LoadFavoritesDB()
        {
            StorageFile file = null;
            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");
                path = file.Path;
                DateTimeOffset created = file.DateCreated;
                DateTimeOffset compare = new DateTimeOffset(new DateTime(2014, 07, 30));

                if (created <= compare)
                {
                    await file.DeleteAsync();
                    file = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");
                }

                LoadFavorites();
            }
            catch (FileNotFoundException)
            {
                MakeFavDB(file);
            }
        }

        public static void LoadFavorites()
        {
            if (MainPageModel.favoritesStops != null) MainPageModel.favoritesStops.Clear();

            var favDB = new SQLiteConnection(path);

            string query = "SELECT * FROM FavoriteData";
            List<FavoriteData> favList = favDB.Query<FavoriteData>(query);

            if (favList.Count > 0)
            {
                foreach (FavoriteData s in favList)
                {
                    if (MainPage.favText.Visibility != Windows.UI.Xaml.Visibility.Collapsed) MainPage.favText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    MainPageModel.favoritesStops.Add(new StopData(s.Name, s.Routes, s.Tags, 0.000, s.Lat, s.Lon, s.Id.ToString()));
                }
            }
            else MainPage.favText.Visibility = Windows.UI.Xaml.Visibility.Visible;
            favDB.Close();

            SyncIDS();
        }

        public static async void MakeFavDB(StorageFile f)
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("favorites.sqlite");
            f = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");
            path = f.Path;

            var favDB = new SQLiteConnection(path);
            favDB.CreateTable<FavoriteData>();
            favDB.Close();
            LoadFavoritesDB();
        }

        public static void AddFavorite(StopData stop)
        {
            var favDB = new SQLiteConnection(path); 
            var s = favDB.Insert(new FavoriteData
                {
                    Name = stop.Name,
                    Routes = stop.Routes,
                    Tags = stop.Tags,
                    Lat = stop.Lat,
                    Lon = stop.Lon
                });
            favDB.Close();
            LoadFavorites();
        }

        public static void RemoveFavorite(StopData stop)
        {
            var favDB = new SQLiteConnection(path);
            string q = "DELETE FROM FavoriteData WHERE Id IS " + stop.FavID;
            favDB.Query<FavoriteData>(q);
            favDB.Close();
            LoadFavorites();
        }

        public static void SyncIDS()
        {
            foreach (StopData a in MainPageModel.favoritesStops)
            {
                foreach (StopData b in MainPageModel.nearbyStops)
                {
                    if (a.Name == b.Name)
                    {
                        b.FavID = a.FavID;
                    }
                }
            }
        }

        public static void FavoriteFromSearch(object selection)
        {
            Stop s = selection as Stop;
            string stop = s.title;

            var db = new SQLiteConnection("db/muni.sqlite");
            string query = "SELECT * FROM BusStops WHERE StopName = \'" + stop + "\'";
            List<BusStop> results = db.Query<BusStop>(query);
            
            //If stop name not found in db, most likely a stop that was a ducplicate and merged so reverse it and search again
            if(results.Count == 0)
            {
                string [] temp = stop.Split('&');
                stop = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));

                query = "SELECT * FROM BusStops WHERE StopName = \'" + stop + "\'";
                results = db.Query<BusStop>(query);
            }
            db.Close();
            var favDB = new SQLiteConnection(path);
            foreach(BusStop x in results)
            {              
                var y = favDB.Insert(new FavoriteData
                {
                    Name = x.StopName,
                    Routes = x.Routes,
                    Tags = x.StopTags,
                    Lat = x.Latitude,
                    Lon = x.Longitude
                });
            }
            favDB.Close();
            LoadFavorites();
        }
    }

    public class BusStop
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string StopName { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Routes { get; set; }
        public string StopTags { get; set; }
        public double Distance { get; set; }
    }

    public class FavoriteData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Routes { get; set; }
        public string Tags { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public FavoriteData() { }

        public FavoriteData(string stopName, string routes, string _tags, string d)
        {
            Name = stopName;
            this.Tags = _tags;
            this.Routes = routes;
        }
    }

    public class Routes
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
