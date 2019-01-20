using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using NexMuni.iOS.Data;
using UIKit;

namespace NexMuni.iOS
{
    class AzureService
    {
        const string dbPath = "redsigned.db";
        MobileServiceClient client;
        IMobileServiceSyncTable<RedesignedInfoItem> redesignedTable;

        public AzureService()
        {
            client = new MobileServiceClient("https://nexmuni.azurewebsites.net/");
        }

        public async Task InitializeAsync()
        {
            var store = new MobileServiceSQLiteStore(dbPath);
            store.DefineTable<RedesignedInfoItem>();
            redesignedTable = client.GetSyncTable<RedesignedInfoItem>();

            // Uses the default conflict handler, which fails on conflict
            await client.SyncContext.InitializeAsync(store);

            await SyncService();

        }

        public async Task SyncService()
        {
            try
            {
                await client.SyncContext.PushAsync();
                await redesignedTable.PullAsync("sync-redesigned", redesignedTable.CreateQuery());
            }
            catch(MobileServicePushFailedException error)
            {
                foreach(var e in error.PushResult.Errors)
                    Debug.WriteLine(e.RawResult);
            }
            catch(MobileServiceInvalidOperationException e)
            {
                Console.Error.WriteLine(@"Sync Failed: {0}", e.Message);
            }
        }
    }
}