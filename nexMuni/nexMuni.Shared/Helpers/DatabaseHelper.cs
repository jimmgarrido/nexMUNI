using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;
using Windows.Storage;
using nexMuni.DataModels;
using nexMuni.ViewModels;
using SQLite;

namespace nexMuni.Helpers
{
    public delegate void ChangedEventHandler();

    public class DatabaseHelper
    {
        private static List<FavoriteData> _favoritesList;

        private static string favoriteDbPath;
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
        public static ChangedEventHandler FavoritesChanged;


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

            var _stopsAsyncConnection = new SQLiteAsyncConnection("muni.sqlite");
            var results = await _stopsAsyncConnection.QueryAsync<Stop>(query);

            //Check results for enough stops
            if (results.Count >= 15)
            {
                return results;
            }
            else return await QueryForNearby(dist += .5);
        }

        public async static Task<List<Stop>> QueryForStop(string stopName)
        {
            string query = "SELECT * FROM BusStops WHERE StopName IS \"" + stopName + "\"";

            var _stopsAsyncConnection = new SQLiteAsyncConnection("muni.sqlite");
            var results = await _stopsAsyncConnection.QueryAsync<Stop>(query);

            if(!results.Any())
            {
                string[] temp = stopName.Split('&');
                if (temp.Count() > 1)
                {
                    stopName = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));
                }

                query = "SELECT * FROM BusStops WHERE StopName IS \"" + stopName + "\"";
                results = await _stopsAsyncConnection.QueryAsync<Stop>(query);
            }
            return results;
        }

        public static async Task< List<string>> QueryForRoutes()
        {
            List<string> list = new List<string>();

            var _stopsAsyncConnection = new SQLiteAsyncConnection("muni.sqlite");
            var query = await _stopsAsyncConnection.QueryAsync<Route>("SELECT * FROM RouteData");

            foreach (var route in query)
            {
                list.Add(route.Title);
            }

            return list;
        }

        public static async Task AddFavoriteAsync(Stop stop)
        {
            var _favoritesAsyncConnection = new SQLiteAsyncConnection(favoriteDbPath);
            await _favoritesAsyncConnection.InsertAsync(new FavoriteData
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
            var _favoritesAsyncConnection = new SQLiteAsyncConnection(favoriteDbPath);
            await _favoritesAsyncConnection.QueryAsync<FavoriteData>(q);
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
 
            var _stopsAsyncConnection = new SQLiteAsyncConnection("muni.sqlite");
            string query = "SELECT * FROM BusStops WHERE StopName = \'" + title + "\'";
            var results = await _stopsAsyncConnection.QueryAsync<Stop>(query);
            
            //If stop name not found in db, most likely a stop that was a duplicate and merged so reverse it and search again
            if(!results.Any())
            {
                string [] temp = title.Split('&');
                title = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));

                query = "SELECT * FROM BusStops WHERE StopName = \'" + title + "\'";
                results = await _stopsAsyncConnection.QueryAsync<Stop>(query);
            }
 
            foreach(Stop fav in results)
            {
                var _favoritesAsyncConnection = new SQLiteAsyncConnection(favoriteDbPath);
                await _favoritesAsyncConnection.InsertAsync(new FavoriteData
                {
                    Name = fav.StopName,
                    Routes = fav.Routes,
                    Tags = fav.StopTags,
                    Lat = fav.Latitude,
                    Lon = fav.Longitude
                });
            }

            await LoadFavoritesAsync();
        }

        private static async Task LoadFavoritesAsync()
        {
            var _favoritesAsyncConnection = new SQLiteAsyncConnection(favoriteDbPath);
            FavoritesList = await _favoritesAsyncConnection.QueryAsync<FavoriteData>("SELECT * FROM FavoriteData");
        }

        private static async Task CheckStopsDatabaseAsync()
        {
            bool dbExists;

            try
            {
                StorageFile muniDb = await ApplicationData.Current.LocalFolder.GetFileAsync("muni.sqlite");
                if (muniDb.DateCreated.Date <= new DateTime(2015, 9, 26))
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
                await dbFile.CopyAsync(ApplicationData.Current.LocalFolder, "muni.sqlite", NameCollisionOption.ReplaceExisting);
            }
        }

        private static async Task CheckFavoritesDatabaseAsync()
        {
            bool dbExists;

            try
            {
                var favDb = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");
                favoriteDbPath = favDb.Path;
                await LoadFavoritesAsync();
                dbExists = true;
            }
            catch
            {
                dbExists = false;
            }

            if (!dbExists) await MakeFavoritesDatabaseAsync();
        }

        private static async Task MakeFavoritesDatabaseAsync()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("favorites.sqlite");
            var favDb = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");

            favoriteDbPath = favDb.Path;
            var _favoritesAsyncConnection = new SQLiteAsyncConnection(favoriteDbPath);
            await _favoritesAsyncConnection.CreateTableAsync<FavoriteData>();
            await LoadFavoritesAsync();
        }
    }
}
