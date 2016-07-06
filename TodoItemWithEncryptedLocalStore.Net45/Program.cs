using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;  // offline sync
using Microsoft.WindowsAzure.MobileServices.Sync;         // offline sync
using Microsoft.WindowsAzure.MobileServices;
using System.IO;

namespace TodoItemWithEncryptedLocalStore
{
    public class Program
    {
        private static IMobileServiceSyncTable<TodoItem> todoTable = Utils.MobileService.GetSyncTable<TodoItem>(); // offline sync

        private static MobileServiceCollection<TodoItem, TodoItem> items;
        static void Main(string[] args)
        {
            TodoItem todoItem = new TodoItem
            {
                Text = "Hi"
            };
            InitLocalStoreAsync().Wait(); // offline sync
            RefreshTodoItems().Wait();
            InsertTodoItem(todoItem).Wait();
            RefreshTodoItems().Wait();
        }


        private static async Task InsertTodoItem(TodoItem todoItem)
        {
            // This code inserts a new TodoItem into the database. When the operation completes
            // and Mobile Apps has assigned an Id, the item is added to the CollectionView
            await todoTable.InsertAsync(todoItem);
            items.Add(todoItem);
        }

        private static async Task RefreshTodoItems()
        {
            // This code refreshes the entries in the list view by querying the TodoItems table.
            // The query excludes completed TodoItems
            items = await todoTable
                .Where(todoItem => todoItem.Complete == false)
                .ToCollectionAsync();
            await Utils.MobileService.SyncContext.PushAsync(); // offline sync
        }

        private static async Task InitLocalStoreAsync()
        {
            if (!Utils.MobileService.SyncContext.IsInitialized)
            {
                var uri = new System.Uri(Path.Combine(Directory.GetCurrentDirectory(), "testSee.db"));
                string testDb = uri.AbsoluteUri + "?hexkey=" + Utils.ToHexString("Hello");
                var store = new MobileServiceSQLiteStore(testDb);
                store.DefineTable<TodoItem>();
                await Utils.MobileService.SyncContext.InitializeAsync(store);
            }
            await SyncAsync();
        }

        private static async Task SyncAsync()
        {
            await Utils.MobileService.SyncContext.PushAsync();
            await todoTable.PullAsync("todoItems", todoTable.CreateQuery());
        }
    }
}
