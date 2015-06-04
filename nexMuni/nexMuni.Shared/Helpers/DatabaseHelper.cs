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

    class DatabaseHelper
    {
        private static SQLiteAsyncConnection _stopsAsyncConnection;
        private static SQLiteAsyncConnection _favoritesAsyncConnection;
        public static List<FavoriteData> FavoritesList { get; private set; }
        public static ChangedEventHandler FavoritesChanged;

        public static async Task CheckDatabasesAsync()
        {
            await CheckStopsDatabaseAsync();
            await CheckFavoritesDatabaseAsync();
        }

        public async static Task<List<BusStopData>> QueryForNearby(double dist)
        {
            //Get search bounds from location and given radius
            double[][] bounds = LocationHelper.MakeBounds(dist);

            //Query database for stops
            string query = "SELECT * FROM BusStops WHERE Longitude BETWEEN " + bounds[3][1] + 
                " AND " + bounds[1][1] + " AND Latitude BETWEEN " + bounds[2][0] + " AND " + bounds[0][0];

            if (_stopsAsyncConnection == null) _stopsAsyncConnection = new SQLiteAsyncConnection("muni.sqlite");
            var results = await _stopsAsyncConnection.QueryAsync<BusStopData>(query);

            //Check results for enough stops
            if (results.Count >= 15)
            {
                return results;
            }
            else return await QueryForNearby(dist += .5);
        }

        public async static Task<List<BusStopData>> QueryForStop(string stopName)
        {
            string query = "SELECT * FROM BusStops WHERE StopName IS \"" + stopName + "\"";
            var results = await _stopsAsyncConnection.QueryAsync<BusStopData>(query);

            if(!results.Any())
            {
                string[] temp = stopName.Split('&');
                if (temp.Count() > 1)
                {
                    stopName = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));
                }

                query = "SELECT * FROM BusStops WHERE StopName IS \"" + stopName + "\"";
                results = await _stopsAsyncConnection.QueryAsync<BusStopData>(query);
            }
            return results;
        }

        public static async Task< List<string>> QueryForRoutes()
        {
            List<string> list = new List<string>();
            var query = await _stopsAsyncConnection.QueryAsync<RouteData>("SELECT * FROM RouteData");

            foreach (var route in query)
            {
                list.Add(route.Title);
            }

            return list;
        }

        public static async Task AddFavoriteAsync(Stop stop)
        {
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
 
            SQLiteAsyncConnection db = new SQLiteAsyncConnection("muni.sqlite");
            string query = "SELECT * FROM BusStops WHERE StopName = \'" + title + "\'";
            List<BusStopData> results = await db.QueryAsync<BusStopData>(query);
            
            //If stop name not found in db, most likely a stop that was a duplicate and merged so reverse it and search again
            if(results.Count == 0)
            {
                string [] temp = title.Split('&');
                title = temp[1].Substring(1) + " & " + temp[0].Substring(0, (temp[0].Length - 1));

                query = "SELECT * FROM BusStops WHERE StopName = \'" + title + "\'";
                results = await db.QueryAsync<BusStopData>(query);
            }
 
            foreach(BusStopData x in results)
            {
                await _favoritesAsyncConnection.InsertAsync(new FavoriteData
                {
                    Name = x.StopName,
                    Routes = x.Routes,
                    Tags = x.StopTags,
                    Lat = x.Latitude,
                    Lon = x.Longitude
                });
            }

            await LoadFavoritesAsync();
        }

        private static async Task LoadFavoritesAsync()
        {
            FavoritesList = await _favoritesAsyncConnection.QueryAsync<FavoriteData>("SELECT * FROM FavoriteData");
            if (FavoritesChanged != null)
            {
                FavoritesChanged();
            }
        }

        private static async Task CheckStopsDatabaseAsync()
        {
            bool dbExists;

            try
            {
                StorageFile muniDb = await ApplicationData.Current.LocalFolder.GetFileAsync("muni.sqlite");
                if (muniDb.DateCreated.Date < new DateTime(2015, 4, 28))
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

            _stopsAsyncConnection = new SQLiteAsyncConnection("muni.sqlite");
        }

        private static async Task CheckFavoritesDatabaseAsync()
        {
            bool dbExists;

            try
            {
                StorageFile favDb = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");
                _favoritesAsyncConnection = new SQLiteAsyncConnection(favDb.Path);
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
            StorageFile favDb = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.sqlite");

            _favoritesAsyncConnection = new SQLiteAsyncConnection(favDb.Path);
            await _favoritesAsyncConnection.CreateTableAsync<FavoriteData>();
            await LoadFavoritesAsync();
        }
    }
}
