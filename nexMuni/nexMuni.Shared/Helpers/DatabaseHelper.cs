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
using nexMuni.DataModels;

namespace nexMuni.Helpers
{
    class DatabaseHelper
    {
        private static string favDBPath = string.Empty;
        private static SQLiteAsyncConnection dbConn;
        public static async Task CheckDatabases()
        {
            await CheckStopsDB();
            await CheckFavDB();
        }

        public async static Task<List<BusStopData>> QueryForNearby(double dist)
        {
            Geopoint point = LocationHelper.phoneLocation.Coordinate.Point;

            //Get search bounds from location and given radius
            double[][] bounds = LocationHelper.MakeBounds(point, dist);

            //Query database for stops
            string query = "SELECT * FROM BusStops WHERE Longitude BETWEEN " + bounds[3][1] + " AND " + bounds[1][1] + " AND Latitude BETWEEN " + bounds[2][0] + " AND " + bounds[0][0];

            if (dbConn == null) dbConn = new SQLiteAsyncConnection("muni.sqlite");
            var results = await dbConn.QueryAsync<BusStopData>(query);

            //Check results for enough stops
            if (results.Count >= 12)
            {
                return results;
            }
            else return await QueryForNearby(dist += .5);
        }

        public static async Task< List<string>> QueryForRoutes()
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection("muni.sqlite");
            var query = await db.QueryAsync<RouteData>("SELECT * FROM RouteData");

            List<string> list = new List<string>();

            foreach (var route in query)
            {
                list.Add(route.Title);
            }

            return list;
        }

        public static async Task<List<FavoriteData>> GetFavorites()
        {
            SQLiteAsyncConnection favDB = new SQLiteAsyncConnection(favDBPath);
            var results = await favDB.QueryAsync<FavoriteData>("SELECT * FROM FavoriteData");
            
            return results;
            //else MainPage.noFavsText.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        public static async Task MakeFavDB(StorageFile f)
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("favorites.sqlite");
            f = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");
            favDBPath = f.Path;

            SQLiteAsyncConnection favDB = new SQLiteAsyncConnection(favDBPath);
            await favDB.CreateTableAsync<FavoriteData>();
            await GetFavorites();
        }

        public static void SyncIDS()
        {
            foreach (StopData a in MainPageModel.FavoritesStops)
            {
                foreach (StopData b in MainPageModel.NearbyStops)
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
            List<BusStopData> results = await db.QueryAsync<BusStopData>(query);
            
            //If stop name not found in db, most likely a stop that was a ducplicate and merged so reverse it and search again
            if(results.Count == 0)
            {
                string [] temp = title.Split('&');
                title = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));

                query = "SELECT * FROM BusStops WHERE StopName = \'" + title + "\'";
                results = await db.QueryAsync<BusStopData>(query);
            }
 
            SQLiteAsyncConnection favDB = new SQLiteAsyncConnection(favDBPath);
            foreach(BusStopData x in results)
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

        private static async Task CheckStopsDB()
        {
            bool dbExists = false;
            try
            {
                StorageFile muniDB = await ApplicationData.Current.LocalFolder.GetFileAsync("muni.sqlite");
                if (muniDB.DateCreated < new DateTime(2015, 4, 19))
                    dbExists = false;
                else
                    dbExists = true;
            }
            catch
            {
                dbExists = false;
            }

            if (!dbExists)
            {
                StorageFile dbFile = await Package.Current.InstalledLocation.GetFileAsync("db\\muni.sqlite");
                await dbFile.CopyAsync(ApplicationData.Current.LocalFolder, "muni.sqlite",NameCollisionOption.ReplaceExisting);
            }
        }

        private static async Task CheckFavDB()
        {
            bool dbExists = false;
            StorageFile file = null;

            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");
                favDBPath = file.Path;
                dbExists = true;
                //await GetFavorites();
            }
            catch
            {
                dbExists = false;
            }

            if(!dbExists)
            {
                await MakeFavDB(file);
            }
        }
    }
}
