using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using System.IO;

namespace TodoItemWithEncryptedLocalStore
{
    public class QSTodoService 
    {
        static QSTodoService instance = new QSTodoService ();
        
        const string localDbPath    = "testSee.db";

        private MobileServiceClient client;
        private IMobileServiceSyncTable<TodoItem> todoTable;

        private QSTodoService ()
        {
            CurrentPlatform.Init ();
            SQLitePCL.CurrentPlatform.Init();

            // Initialize the Mobile Service client with the Mobile App URL, Gateway URL and key
            client = Utils.MobileService;

            // Create an MSTable instance to allow us to work with the TodoItem table
            todoTable = client.GetSyncTable <TodoItem> ();
        }

        public static QSTodoService DefaultService {
            get {
                return instance;
            }
        }

        public List<TodoItem> Items { get; private set;}

        public async Task InitializeStoreAsync()
        {
            var localDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "testNoSee.db");
            //DO NOT HARDCODE PASSWORD IN YOUR APPLICATION
            string testDb = localDbPath + "?hexkey="+Utils.ToHexString("Hello");

            var store = new MobileServiceSQLiteStore(testDb);
            store.DefineTable<TodoItem>();

            // Uses the default conflict handler, which fails on conflict
            // To use a different conflict handler, pass a parameter to InitializeAsync. For more details, see http://go.microsoft.com/fwlink/?LinkId=521416
            await client.SyncContext.InitializeAsync(store);
        }

        public async Task SyncAsync(bool pullData = false)
        {
            try
            {
                await client.SyncContext.PushAsync();

                if (pullData) {
                    await todoTable.PullAsync("allTodoItems", todoTable.CreateQuery()); // query ID is used for incremental sync
                }
            }

            catch (MobileServiceInvalidOperationException e)
            {
                Console.Error.WriteLine(@"Sync Failed: {0}", e.Message);
            }
        }

        public async Task<List<TodoItem>> RefreshDataAsync ()
        {
            try {
                // update the local store
                // all operations on todoTable use the local database, call SyncAsync to send changes
                await SyncAsync(pullData: true); 							

                // This code refreshes the entries in the list view by querying the local TodoItems table.
                // The query excludes completed TodoItems
                Items = await todoTable
                        .Where (todoItem => todoItem.Complete == false).ToListAsync ();

            } catch (MobileServiceInvalidOperationException e) {
                Console.Error.WriteLine (@"ERROR {0}", e.Message);
                return null;
            }

            return Items;
        }

        public async Task InsertTodoItemAsync (TodoItem todoItem)
        {
            try {                
                await todoTable.InsertAsync (todoItem); // Insert a new TodoItem into the local database. 
                await SyncAsync(); // send changes to the mobile service

                Items.Add (todoItem); 

            } catch (MobileServiceInvalidOperationException e) {
                Console.Error.WriteLine (@"ERROR {0}", e.Message);
            }
        }

        public async Task CompleteItemAsync (TodoItem item)
        {
            try {
                item.Complete = true; 
                await todoTable.UpdateAsync (item); // update todo item in the local database
                await SyncAsync(); // send changes to the mobile service

                Items.Remove (item);

            } catch (MobileServiceInvalidOperationException e) {
                Console.Error.WriteLine (@"ERROR {0}", e.Message);
            }
        }
    }
}

