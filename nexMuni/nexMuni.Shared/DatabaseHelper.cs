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
using System.Collections.ObjectModel;
using Windows.ApplicationModel;

namespace nexMuni
{
    class DatabaseHelper
    {
        protected static string favDBPath = string.Empty;

        public static async Task CheckDB()
        {
            bool dbExists = false;
            try
            {
                StorageFile muniDB = await ApplicationData.Current.LocalFolder.GetFileAsync("muni.sqlite");
                dbExists = true;
            }
            catch
            {
                dbExists = false;
            }

            if(!dbExists)
            {
                StorageFile dbFile = await Package.Current.InstalledLocation.GetFileAsync("db\\muni.sqlite");
                await dbFile.CopyAsync(ApplicationData.Current.LocalFolder);
            }

        }

        public async static Task QueryForNearby(Geopoint point, double dist)
        {
            //Get search bounds from location and given radius
            double[][] bounds = LocationHelper.MakeBounds(point, dist);

            //Query database for stops
            string query = "SELECT * FROM BusStops WHERE Longitude BETWEEN " + bounds[3][1] + " AND " + bounds[1][1] + " AND Latitude BETWEEN " + bounds[2][0] + " AND " + bounds[0][0];
            SQLiteAsyncConnection db = new SQLiteAsyncConnection("muni.sqlite");
            var results = await db.QueryAsync<BusStop>(query);

            //Check results for enough stops. If less than 5 returned, call method again with larger radius
            if (results.Count < 5)
            {
                QueryForNearby(point, dist += 0.50);
            }
            else MainPageModel.DisplayResults(results);
        }

        public static async Task<List<string>> QueryForRoutes()
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection("muni.sqlite");
            var query = await db.QueryAsync<Routes>("SELECT * FROM Routes");

            List<string> list = new List<string>();

            foreach (var route in query)
            {
                list.Add(route.Title);
            }

            return list;
        }

        public static async Task CheckFavDB()
        {
            StorageFile file = null;
            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");
                favDBPath = file.Path;

                GetFavorites();
            }
            catch (FileNotFoundException)
            {
                MakeFavDB(file);
            }
        }

        public static async Task GetFavorites()
        {
            if (MainPageModel.favoritesStops != null) MainPageModel.favoritesStops.Clear();

            SQLiteAsyncConnection favDB = new SQLiteAsyncConnection(favDBPath);
            var results = await favDB.QueryAsync<FavoriteData>("SELECT * FROM FavoriteData");
            if (results.Count > 0) MainPageModel.DisplayResults(results);
            else MainPage.noFavsText.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        public static async Task MakeFavDB(StorageFile f)
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("favorites.sqlite");
            f = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");
            favDBPath = f.Path;

            SQLiteAsyncConnection favDB = new SQLiteAsyncConnection(favDBPath);
            await favDB.CreateTableAsync<FavoriteData>();
            GetFavorites();
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

        public static async Task AddFavorite(StopData stop)
        {
            SQLiteAsyncConnection favDB = new SQLiteAsyncConnection(favDBPath); 
            await favDB.InsertAsync(new FavoriteData
                {
                    Name = stop.Name,
                    Routes = stop.Routes,
                    Tags = stop.Tags,
                    Lat = stop.Lat,
                    Lon = stop.Lon
                });
            await GetFavorites();
        }

        public static async Task RemoveFavorite(StopData stop)
        {
            SQLiteAsyncConnection favDB = new SQLiteAsyncConnection(favDBPath);
            string q = "DELETE FROM FavoriteData WHERE Id IS " + stop.FavID;
            await favDB.QueryAsync<FavoriteData>(q);
            await GetFavorites();
        }


        public static async Task FavoriteFromSearch(StopData selection)
        {
            string title = selection.Name;
            if (title.Contains("Inbound"))
            {
                title = title.Replace(" Inbound", "");
            }
            if (title.Contains("Outbound"))
            {
                title = title.Replace(" Outbound", "");
            }
 
            SQLiteAsyncConnection db = new SQLiteAsyncConnection("muni.sqlite");
            string query = "SELECT * FROM BusStops WHERE StopName = \'" + title + "\'";
            List<BusStop> results = await db.QueryAsync<BusStop>(query);
            
            //If stop name not found in db, most likely a stop that was a ducplicate and merged so reverse it and search again
            if(results.Count == 0)
            {
                string [] temp = title.Split('&');
                title = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));

                query = "SELECT * FROM BusStops WHERE StopName = \'" + title + "\'";
                results = await db.QueryAsync<BusStop>(query);
            }
 
            SQLiteAsyncConnection favDB = new SQLiteAsyncConnection(favDBPath);
            foreach(BusStop x in results)
            {              
                await favDB.InsertAsync(new FavoriteData
                {
                    Name = x.StopName,
                    Routes = x.Routes,
                    Tags = x.StopTags,
                    Lat = x.Latitude,
                    Lon = x.Longitude
                });
            }
            
            await GetFavorites();
        }

        public static async Task RemoveSearch(StopData selection)
        {
            SQLiteAsyncConnection favDB = new SQLiteAsyncConnection(favDBPath);
            string q = "DELETE FROM FavoriteData WHERE Id IS " + selection.FavID;
            await favDB.QueryAsync<FavoriteData>(q);
            await GetFavorites();
        }
    }

    [Table("BusStops")]
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
        public int ID { get; set; }
        public string Title { get; set; }
    }
}
