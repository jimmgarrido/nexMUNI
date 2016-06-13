using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Windows.Storage;
using nexMuni.DataModels;
using SQLite.Net.Async;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using SQLite.Net.Interop;
using System.IO;
using Windows.ApplicationModel;

namespace nexMuni.Helpers
{
    public delegate void ChangedEventHandler();

    public class DatabaseHelper
    {
        private static List<FavoriteData> _favoritesList;
        private static string favoriteDbPath = string.Empty;
        private static string muniDbPath = string.Empty;
        public static ChangedEventHandler FavoritesChanged;
        public static SQLiteAsyncConnection favoritesConnection;

        public static List<FavoriteData> FavoritesList
        {
            get
            {
                return _favoritesList;
            }
            set
            {
                _favoritesList = value;
                if (FavoritesChanged != null)
                {
                    FavoritesChanged();
                }
            }
        }

        public static async Task CheckDatabasesAsync()
        {
            await CheckStopsDatabaseAsync();
            await CheckFavoritesDatabaseAsync();
        }

        public async static Task<List<Stop>> QueryForNearby(double dist)
        {
            //Get search bounds from location and given radius
            double[][] bounds = LocationHelper.MakeBounds(dist);

            //Query database for stops
            string query = "SELECT * FROM BusStops WHERE Longitude BETWEEN " + bounds[3][1] +
                " AND " + bounds[1][1] + " AND Latitude BETWEEN " + bounds[2][0] + " AND " + bounds[0][0];

            //var _stopsAsyncConnection = new SQLiteAsyncConnection("muni.sqlite");
            var connection = new SQLiteAsyncConnection(() => DbConnection(muniDbPath));
            var results = await connection.QueryAsync<Stop>(query);
            connection = null;

            //Check results for enough stops
            if (results.Count >= 15)
            {
                return results;
            }
            else return await QueryForNearby(dist += .5);
        }

        public async static Task<List<Stop>> QueryForStop(string selectedStop)
        {
            //string query = "SELECT * FROM BusStops WHERE StopName IS \"" + selectedStop + "\"";

            //var _stopsAsyncConnection = new SQLiteAsyncConnection("muni.sqlite");
            var connection = new SQLiteAsyncConnection(() => DbConnection(muniDbPath));
            var results = await connection.Table<Stop>().Where(s => s.StopName == selectedStop).ToListAsync();
            //var results = await connection.QueryAsync<Stop>(query);

            if (!results.Any())
            {
                string[] temp = selectedStop.Split('&');
                if (temp.Count() > 1)
                {
                    selectedStop = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));
                }

                results = await connection.Table<Stop>().Where(s => s.StopName == selectedStop).ToListAsync();
            }

            connection = null;
            return results;
        }

        public static async Task<List<Route>> QueryForRoutes()
        {
            //var list = new List<string>();
            var results = new List<Route>();

            //var _stopsAsyncConnection = new SQLiteAsyncConnection("muni.sqlite");
            var _stopsAsyncConnection = new SQLiteAsyncConnection(() => DbConnection(muniDbPath));
            results = await _stopsAsyncConnection.Table<Route>().ToListAsync();
            _stopsAsyncConnection = null;
            //foreach (var route in query)
            //{
            //    list.Add(route.Title);
            //}

            return results;
        }

        public static async Task AddFavoriteAsync(Stop stop)
        {
            //var _favoritesAsyncConnection = new SQLiteAsyncConnection(favoriteDbPath);
            favoritesConnection = new SQLiteAsyncConnection(() => DbConnection(favoriteDbPath));

            await favoritesConnection.InsertAsync(new FavoriteData
                {
                    Name = stop.StopName,
                    Routes = stop.Routes,
                    Tags = stop.StopTags,
                    Lat = stop.Latitude,
                    Lon = stop.Longitude
                });

            await LoadFavoritesAsync();
        }

        public static async Task RemoveFavoriteAsync(Stop stop)
        {
            string q = "DELETE FROM FavoriteData WHERE Id IS " + stop.favId;
            //var _favoritesAsyncConnection = new SQLiteAsyncConnection(favoriteDbPath);
            favoritesConnection = new SQLiteAsyncConnection(() => DbConnection(favoriteDbPath));

            await favoritesConnection.QueryAsync<FavoriteData>(q);
            await LoadFavoritesAsync();
        }

        public static async Task FavoriteSearchAsync(Stop stop)
        {
            string title = stop.StopName;
            if (title.Contains("Inbound"))
            {
                title = title.Replace(" Inbound", "");
            }
            if (title.Contains("Outbound"))
            {
                title = title.Replace(" Outbound", "");
            }

            //var _stopsAsyncConnection = new SQLiteAsyncConnection("muni.sqlite");
            var connection = new SQLiteAsyncConnection(() => DbConnection(muniDbPath));

            //string query = "SELECT * FROM BusStops WHERE StopName = \'" + title + "\'";
            var results = await connection.Table<Stop>().Where(s => s.StopName == title).ToListAsync();
            
            //If stop name not found in db, most likely a stop that was a duplicate and merged so reverse it and search again
            if(!results.Any())
            {
                string [] temp = title.Split('&');
                title = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));

                //query = "SELECT * FROM BusStops WHERE StopName = \'" + title + "\'";
                results = await connection.Table<Stop>().Where(s => s.StopName == title).ToListAsync();
            }

            foreach (Stop fav in results)
            {
                await AddFavoriteAsync(stop);
            }
        }

        private static async Task LoadFavoritesAsync()
        {
            //var _favoritesAsyncConnection = new SQLiteAsyncConnection(favoriteDbPath);
            //var _favoritesAsyncConnection = new SQLiteAsyncConnection(() => new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(favoriteDbPath, false)));
            //var _favoritesAsyncConnection = new SQLiteConnection(new SQLitePlatformWinRT(), favoriteDbPath);
            FavoritesList = await favoritesConnection.Table<FavoriteData>().ToListAsync();
            favoritesConnection = null;
        }

        private static async Task CheckStopsDatabaseAsync()
        {
            bool dbExists = true;
            var expiryDate = DateTime.Today - (new TimeSpan(7, 0, 0, 0)); 

            try
            {
                StorageFile muniDb = await ApplicationData.Current.LocalFolder.GetFileAsync("muni.sqlite");

                if (muniDb.DateCreated.Date <= new DateTime(2016, 1, 23))
                    throw new Exception("Database out of date");
            }
            catch (Exception ex)
            {
                dbExists = false;
            }

            if (!dbExists)
            {
                try
                {
                    var refreshClient = new DataRefreshHelper();
                    await refreshClient.RefreshDataAsync();
                }
                catch (Exception e)
                {
                    StorageFile dbFile = await Package.Current.InstalledLocation.GetFileAsync("db\\muni.sqlite");
                    await dbFile.CopyAsync(ApplicationData.Current.LocalFolder, "muni.sqlite", NameCollisionOption.ReplaceExisting);
                }
            }

            muniDbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "muni.sqlite");
        }

        private static async Task CheckFavoritesDatabaseAsync()
        {
            /**
             * Keep favorites database in the roaming folder so it synced across devices/installs
             * Move over exisiting user's favorites from local to romaing folder if it exisits 
             * */

            bool dbExists = false;
            bool isLocal = false;

            try
            {
                var favDb = await ApplicationData.Current.RoamingFolder.GetFileAsync("favorites.sqlite");
                dbExists = true;
                favoriteDbPath = favDb.Path;

                favoritesConnection = new SQLiteAsyncConnection(() => DbConnection(favoriteDbPath));
                await LoadFavoritesAsync();
            }
            catch
            {
                dbExists = false;
            }

            // Check if there is an exisiting database in the local folder that needs to be migrated
            if(!dbExists)
            {
                StorageFile localFavDb = null;

                try
                {
                    localFavDb = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");
                    isLocal = true;
                }
                catch
                {
                    isLocal = false;
                }

                if(isLocal)
                {
                    await localFavDb.CopyAsync(ApplicationData.Current.RoamingFolder, "favorites.sqlite", NameCollisionOption.ReplaceExisting);
                    var favDb = await ApplicationData.Current.RoamingFolder.GetFileAsync("favorites.sqlite");
                    favoriteDbPath = favDb.Path;

                    favoritesConnection = new SQLiteAsyncConnection(() => DbConnection(favoriteDbPath));
                    await LoadFavoritesAsync();
                }
                else
                {
                    await MakeFavoritesDatabaseAsync();
                }
            }
        }

        private static async Task MakeFavoritesDatabaseAsync()
        {
            await ApplicationData.Current.RoamingFolder.CreateFileAsync("favorites.sqlite");
            var favDb = await ApplicationData.Current.RoamingFolder.GetFileAsync("favorites.sqlite");
            favoriteDbPath = favDb.Path;

            favoritesConnection = new SQLiteAsyncConnection(() => DbConnection(favoriteDbPath));
            await favoritesConnection.CreateTableAsync<FavoriteData>();
            await LoadFavoritesAsync();
        }

        private static SQLiteConnectionWithLock DbConnection(string path)
        {
            return new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(path, false));
        }
    }
}
