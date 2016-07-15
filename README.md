---
services: app-service\mobile
platforms: dotnet, xamarin
author: pragnagopa
---
# How to use SQLite Encryption Extension with Azure Mobile Apps
[Azure Mobile Apps](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-value-prop/) provides an offline sync feature that makes it easy to build mobile apps that work without a network connection. When apps use this feature, end users can still create and modify data, even when the app is in an offline mode. The changes are saved to a local store and can be synced to your Mobile App backend when the app is back online. See [Offline Data Sync in Azure Mobile Apps](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-offline-data-sync) for more details. 

Offline sync uses a [local store](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-offline-data-sync/#what-is-a-local-store) to persist data on the client device, where the specific storage implementation can be customized. The Mobile Apps client SDK provides local stores based on SQLite (Windows, Xamarin, and Android) and Core Data (iOS native). 

SQLite does not have any built-in encryption support, but there are a number of extensions that provide encryption.  I’ll show how to use the [SQLite Encryption Extension (SEE)](http://www.hwaci.com/sw/sqlite/see.html) to use an encrypted database in your mobile app, with support for both reads and writes. Note that SEE is not the only encryption option for Mobile Apps; For instance, you can define a local store that uses [SQLCipher](https://www.zetetic.net/sqlcipher/documentation/) for encryption. 

This sample requires you to purchase a license for [SEE](http://www.hwaci.com/sw/sqlite/see.html). SEE is an add-on to the public domain version of SQLite and does not require a custom local store implementation.

### Prerequisites
To build this sample
* [Buy license for SEE](http://www.hwaci.com/cgi-bin/see-step1)
* From the [downloads page](https://www.sqlite.org/download.html)
    * Get the latest version of [The SQLite Amalgamation](https://www.sqlite.org/amalgamation.html)
    * Precompiled Binaries for Windows (this sample uses x86 version)
* [Obtain code for building sqlite for Android](https://www.sqlite.org/android/doc/trunk/www/install.wiki#obtaincode)
    
### Build sqlite3 dll with SEE 
> Note: This sample demostrates one of the ways to build sqlite3. You can find complete instructions at: [How to compile and use SEE](https://www.sqlite.org/see/doc/trunk/www/readme.wiki)

* Append of the contents of see.c file to the end of the sqlite3.c file. Save the file as sqlite3.c
* Copy following files into Common folder
      * sqlite3.c with SEE code
      * sqlite3.def obtained from precompiled binaries for windows
      * sqlite3.h obtained when you purchased SEE files

* Xamarin.Android
      * [Build native library with SEE](https://www.sqlite.org/android/doc/trunk/www/see.wiki)
      * By default, native library is built for armeabi. This sample runs on emulator which needs native library built for x86 :
      ```sh           
       cd SQLite_Android_Bindings\sqlite3\src\main
       <Path to Android ndk-build>\ndk-build.cmd APP_ABI=x86
      ```
      * Copy the generated native library for x86 from SQLite_Android_Bindings\sqlite3\src\main\libs\x86 to TodoItemWithEncryptedLocalStore.Android\lib\x86
* Windows      
      * Open TodoItemWithEncryptedLocalStore.sln in visual studio
         * Right click on sqlite3.c file in sqlite3WindowsRuntimeComponent project
         * Open properties page for sqlite3.c, under C/C++ **-->** General, delete /ZW from Consume Windows Runtime extension
      This sample will build the sqlite3.dll that can be used in an **Universal Windows App**.
* Xamarin.iOS
      * [Configure Visual Studio to build iOS static library](https://msdn.microsoft.com/en-us/library/mt147405.aspx)
      * Open TodoItemWithEncryptedLocalStore.sln in visual studio
          * Build sqlite3iOS project - x86 for simulator, ARM for the device. This will generate static library libsqlite3iOS.a on the mac. You can find full file path in the output window.
          * Copy the generated static library from connected mac /<vcremoteFolder>/libsqlite3iOS.a to Common folder. Copy the library with the right architechture. x86 for running on the simulator, ARM for the device. Mismatch in the architechtures will result in linker errors when building the sample.
          
### Build and Run the sample
Update Mobile App service URL in Common\Utils.cs. You can now build and run the sample.

This sample is based on the [Azure Mobile Apps Getting started tutorial](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-xamarin-android-get-started/). It is modified to use sqlite with SEE to use encrypted local store. Local database is created using [Uri filename](https://www.sqlite.org/uri.html) with password passed as a query parameter **hexkey**. Here is the code that initializes local store:

```sh
 private async Task InitLocalStoreAsync()
        {
            if (!Utils.MobileService.SyncContext.IsInitialized)
            {
                var uri = new System.Uri(Path.Combine(ApplicationData.Current.LocalFolder.Path,"testSee.db"));
                string testDb = uri.AbsoluteUri + "?hexkey=" + Utils.ToHexString("Hello");
                var store = new MobileServiceSQLiteStore(testDb);
                store.DefineTable<TodoItem>();
                await Utils.MobileService.SyncContext.InitializeAsync(store);
            }

            await SyncAsync();
        }
```

Once the local database is created with a password it is encrypted. If you try to use that database without providing a valid password, it would fail with **Error: file is encrypted or is not a database**.  Try it out - Remove the hexkey query parameter from the testdb string and run the sample.

### Tips and useful links
* Build and use [CLI for SEE](https://www.sqlite.org/see/doc/trunk/www/readme.wiki) for additional options. For eg: You can update/remove password for encrypted database using the rekey option.
* Guide on how to [Interop with Native Libraries](http://www.mono-project.com/docs/advanced/pinvoke/) in Xamarin
* In order to load custom version of sqlite dll instead of the one that is included in Android platform, Xamarin.Adroid application needs to add app.config
    ```sh
    <dllmap dll="sqlite3" target="libsqliteX.so" />
    ```
    this config file is included in the apk package only when EmbedAssembliesIntoApk is set to true in csproj file
    ```sh
    <EmbedAssembliesIntoApk>True</EmbedAssembliesIntoApk>
    ```
    EmbedAssembliesIntoApk is false by default in debug builds. When debugging the app, set this to true to load the sqlite      dll that you built with SEE

* [Android debug log](https://developer.xamarin.com/guides/android/deployment,_testing,_and_metrics/android_debug_log/) shows  detailed logs on location of the assemblies being loaded into the application. You can use this to verify sqlite3 that you built is loaded into the app.

* For Xamarin.Android, set the build configuration to match the native library architechture. For eg: this sample uses x86 version of native library which is copied to TodoItemWithEncryptedLocalStore.Android\lib\x86, you need to run/debug x86 version of the app as the path to load the user libraries depends on the architecture. Mismatch in the architechture will cause dll not found errors.

* Following comiple options are used to build sqlite3.dll for Windows
    ```sh
    THREADSAFE
    SQLITE_ENABLE_COLUMN_METADATA
    SQLITE_ENABLE_RTREE
    SQLITE_HAS_CODEC
    SQLITE_OS_WINRT
    ```
    To edit these options, open property pages for sqlite3WindowsRuntimeComponent project and update **Preprocessor Definitions** under C/C++ **-->** Preprocessor
* Following comiple options are used to build a sqlite3 static library for Xamarin.iOS
    ```sh
    THREADSAFE
    SQLITE_ENABLE_COLUMN_METADATA
    SQLITE_ENABLE_RTREE
    SQLITE_HAS_CODEC
    
    ```
   To edit these options, open property pages for sqlite3iOS project and update **Preprocessor Definitions** under C/C++ **-->** Preprocessor
* [How to link Native Libraries in Xamarin.iOS](https://developer.xamarin.com/guides/ios/advanced_topics/native_interop/). See build options on TodoItemWithEncryptedLocalStore.iOS project to update -gcc_flags used for linking libraries.
* This sample uses modified version of the [SQLitePCL](https://sqlitepcl.codeplex.com/). Notice the DllImport statements use "__Internal" which force the application to load the static library linked to the app instead of the system library:
      ```sh
      
      [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_open")]
      
      internal static extern int sqlite3_open(IntPtr filename, out IntPtr db);
      
      ```
