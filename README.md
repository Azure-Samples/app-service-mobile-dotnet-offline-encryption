---
services: app-service\mobile
platforms: dotnet, xamarin
author: pragnagopa
---
# How to use SQLite Encryption Extension with Azure Mobile Apps

[Azure Mobile Apps](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-value-prop/) provides client SDKs with a SQLite based local store implementation for Windows, Xamarin and Android.

More details at: [Offline Data Sync in Azure Mobile Apps](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-offline-data-sync).

The [SQLite Encryption Extension (SEE)](http://www.hwaci.com/sw/sqlite/see.html) is an add-on to the public domain version of SQLite that allows an application to read and write encrypted database files.

### Prerequisites
To build this sample
* [Buy license for SEE](http://www.hwaci.com/cgi-bin/see-step1)
* From the [downloads page](https://www.sqlite.org/download.html)
    * Get the latest version of [The SQLite Amalgamation](https://www.sqlite.org/amalgamation.html)
    * Precompiled Binaries for Windows (this sample uses x86 version)
* [Obtain code for building sqlite for Android](https://www.sqlite.org/android/doc/trunk/www/install.wiki#obtaincode)
    
### Build sqlite3 dll with SEE 
> Note: This sample demostrates just one of the ways to build sqlite3. Detailed instructions at
> [How to compile sqlite](https://www.sqlite.org/howtocompile.html), [How to compile and use SEE](https://www.sqlite.org/see/doc/trunk/www/readme.wiki)
* Append of the contents of see.c file to the end of the sqlite3.c file. Save the file as sqlite3.c
    * For Xamarin.Android
        * [Build native library with SEE](https://www.sqlite.org/android/doc/trunk/www/see.wiki)
        * By default, native library is built for armeabi. This sample runs on emulator which needs native library built for x86 :
      ```sh           
       cd SQLite_Android_Bindings\sqlite3\src\main
       <Path to Android ndk-build>\ndk-build.cmd APP_ABI=x86
      ```
      * Copy generated native library for x86 from SQLite_Android_Bindings\sqlite3\src\main\libs\x86 to TodoItemWithEncryptedLocalStore\TodoItemWithEncryptedLocalStore.Android\lib\x86
    * For Windows      
       *  Copy following files into TodoItemWithEncryptedLocalStore\Common folder
         * sqlite3.c with SEE code
         * sqlite3.def obtained from precompiled binaries for windows
         * sqlite3.h obtained when you purchased SEE files
       * Open TodoItemWithEncryptedLocalStore.sln in visual studio
         * Right click on sqlite3.c file in sqlite3WindowsRuntimeComponent project
         * Open properties page for sqlite3.c, under C/C++ **-->** General, delete /ZW from Consume Windows Runtime extension
    This sample will build the sqlite3.dll that can be used in an **Universal Windows App**.

### Build and Run the sample
Update Mobile App service URL in TodoItemWithEncryptedLocalStore\Common\Utils.cs. You can now build and run the sample.

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

* For Xamarin.Android, set the build configuration to match the native library architechture. For eg: this sample uses x86 version of native library which is copied to TodoItemWithEncryptedLocalStore\TodoItemWithEncryptedLocalStore.Android\lib\x86, you need to run/debug x86 version of the app as the path to load the user libraries depends on the architecture. Mismatch in the architechture will cause dll not found errors.

* Following comiple options are used to build sqlite3.dll for windows
    ```sh
    THREADSAFE
    SQLITE_ENABLE_COLUMN_METADATA
    SQLITE_ENABLE_RTREE
    SQLITE_HAS_CODEC
    SQLITE_OS_WINRT
    ```
    To edit these options, open property pages for sqlite3WindowsRuntimeComponent project and update **Preprocessor Definitions** under C/C++ **-->** Preprocessor
    
