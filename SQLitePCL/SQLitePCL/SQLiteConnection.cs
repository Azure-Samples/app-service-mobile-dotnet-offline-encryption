// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.
// Note: Original source code at https://sqlitepcl.codeplex.com/SourceControl/latest

namespace SQLitePCL
{
    using System;
    using System.Collections.Generic;

    public class SQLiteConnection : ISQLiteConnection
    {
        private static bool temporaryDirectorySet = false;

        private static object syncTDS = new object();

        private IPlatformMarshal platformMarshal;

        private IPlatformStorage platformStorage;

        private ISQLite3Provider sqlite3Provider;

        private IntPtr db;

        private bool disposed = false;

        private IDictionary<string, Delegate> functionDelegates = new Dictionary<string, Delegate>();

        private IDictionary<string, Delegate> aggregateStepDelegates = new Dictionary<string, Delegate>();

        private IDictionary<string, Delegate> aggregateFinalDelegates = new Dictionary<string, Delegate>();

        private IDictionary<Guid, IDictionary<string, object>> aggregateContextDataDic = new Dictionary<Guid, IDictionary<string, object>>();

        private IDictionary<string, Delegate> collationDelegates = new Dictionary<string, Delegate>();

        public SQLiteConnection(string fileName)
            : this(fileName, SQLiteOpen.READWRITE, true)
        {
        }

        public SQLiteConnection(string fileName, SQLiteOpen openFlag)
            : this(fileName, openFlag, true)
        {
        }

        private SQLiteConnection(string fileName, SQLiteOpen openFlag, bool setTemporaryDirectory)
        {
            this.platformMarshal = Platform.Instance.PlatformMarshal;
            this.platformStorage = Platform.Instance.PlatformStorage;
            this.sqlite3Provider = Platform.Instance.SQLite3Provider;

            if (setTemporaryDirectory)
            {
                this.SetTemporaryDirectory();
            }

            var localFilePath = string.Empty;

            if (fileName.Trim().ToLowerInvariant() == ":memory:")
            {
                localFilePath = ":memory:";
            }
            else if (fileName.Trim() != string.Empty)
            {
                localFilePath = this.platformStorage.GetLocalFilePath(fileName);
            }

            var fileNamePtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(localFilePath);

            int flags;

            switch (openFlag)
            {
                case SQLiteOpen.READONLY:
                    // URI|DONTCREATE|READONLY
                    flags = 0x41;
                    break;
                case SQLiteOpen.READWRITE:
                    // URI|CREATE|READWRITE
                    flags = 0x46;
                    break;
                default:
                    // URI|CREATE|READWRITE
                    flags = 0x46;
                    break;
            }

            try
            {
                var openResult = (SQLiteResult)this.sqlite3Provider.Sqlite3Open(fileNamePtr, out this.db, flags);

                if (openResult != SQLiteResult.OK)
                {
                    if (this.db != IntPtr.Zero)
                    {
                        var errmsgPtr = this.sqlite3Provider.Sqlite3Errmsg(this.db);

                        var errmsg = this.platformMarshal.MarshalStringNativeUTF8ToManaged(errmsgPtr);

                        this.sqlite3Provider.Sqlite3CloseV2(this.db);

                        throw new SQLiteException("Unable to open the database file: " + fileName + " Details: " + errmsg);
                    }
                    else
                    {
                        throw new SQLiteException("Unable to open the database file: " + fileName + " Details: " + openResult.ToString());
                    }
                }
            }
            catch (SQLiteException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SQLiteException("Unable to open the database file: " + fileName, ex);
            }
            finally
            {
                if (fileNamePtr != IntPtr.Zero)
                {
                    this.platformMarshal.CleanUpStringNativeUTF8(fileNamePtr);
                }
            }
        }

        ~SQLiteConnection()
        {
            this.Dispose(false);
        }

        public ISQLiteStatement Prepare(string sql)
        {
            IntPtr stm;

            int sqlLength;

            var sqlPtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(sql, out sqlLength);

            try
            {
                if (this.sqlite3Provider.Sqlite3PrepareV2(this.db, sqlPtr, sqlLength, out stm, IntPtr.Zero) != (int)SQLiteResult.OK)
                {
                    var errmsgPtr = this.sqlite3Provider.Sqlite3Errmsg(this.db);

                    var errmsg = this.platformMarshal.MarshalStringNativeUTF8ToManaged(errmsgPtr);

                    throw new SQLiteException("Unable to prepare the sql statement: " + sql + " Details: " + errmsg);
                }

                return new SQLiteStatement(this, stm);
            }
            catch (SQLiteException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SQLiteException("Unable to prepare the sql statement: " + sql, ex);
            }
            finally
            {
                if (sqlPtr != IntPtr.Zero)
                {
                    this.platformMarshal.CleanUpStringNativeUTF8(sqlPtr);
                }
            }
        }

        public void CreateFunction(string name, int numberOfArguments, Function function, bool deterministic)
        {
            name = name.ToUpperInvariant();

            var nativeFunction = new FunctionNative((context, numberArguments, nativeArguments) =>
            {
                object[] mangedArguments = ObtainManagedArguments(nativeArguments);

                try
                {
                    var result = function.Invoke(mangedArguments);

                    SetResult(context, result);
                }
                catch (Exception ex)
                {
                    SetError(context, ex);
                }
            });

            var functionDelegate = this.platformMarshal.ApplyNativeCallingConventionToFunction(nativeFunction);
            this.functionDelegates[name] = functionDelegate;

            var funcPtr = this.platformMarshal.MarshalDelegateToNativeFunctionPointer(functionDelegate);

            int nameLength;
            var namePtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(name, out nameLength);

            try
            {
                this.sqlite3Provider.Sqlite3CreateFunction(this.db, namePtr, numberOfArguments, deterministic, funcPtr);
            }
            finally
            {
                if (namePtr != IntPtr.Zero)
                {
                    this.platformMarshal.CleanUpStringNativeUTF8(namePtr);
                }
            }
        }

        public void CreateAggregate(string name, int numberOfArguments, AggregateStep step, AggregateFinal final)
        {
            name = name.ToUpperInvariant();

            var nativeAggregateStep = new AggregateStepNative((context, numberArguments, nativeArguments) =>
            {
                var contextGuidPtr = this.sqlite3Provider.Sqlite3AggregateContext(context, 16);

                if (contextGuidPtr != IntPtr.Zero)
                {
                    var contextGuidBytes = new byte[16];
                    this.platformMarshal.Copy(contextGuidPtr, contextGuidBytes, 0, 16);
                    var contextGuid = new Guid(contextGuidBytes);

                    if (contextGuid == Guid.Empty)
                    {
                        contextGuid = Guid.NewGuid();
                        contextGuidBytes = contextGuid.ToByteArray();
                        this.platformMarshal.Copy(contextGuidBytes, contextGuidPtr, 0, 16);
                    }

                    object[] mangedArguments = ObtainManagedArguments(nativeArguments);

                    // TODO
                    if (!this.aggregateContextDataDic.ContainsKey(contextGuid))
                    {
                        this.aggregateContextDataDic[contextGuid] = new Dictionary<string, object>();
                    }

                    var aggregateContextData = this.aggregateContextDataDic[contextGuid];
                    try
                    {
                        step.Invoke(aggregateContextData, mangedArguments);
                    }
                    catch (Exception ex)
                    {
                        SetError(context, ex);
                    }
                }
                else
                {
                    SetError(context, new Exception("Unable to initialize aggregate context."));
                }
            });

            var aggregateStepDelegate = this.platformMarshal.ApplyNativeCallingConventionToAggregateStep(nativeAggregateStep);
            this.aggregateStepDelegates[name] = aggregateStepDelegate;

            var aggregateStepPtr = this.platformMarshal.MarshalDelegateToNativeFunctionPointer(aggregateStepDelegate);

            var nativeAggregateFinal = new AggregateFinalNative((context) =>
            {
                IDictionary<string, object> aggregateContextData = new Dictionary<string, object>();

                var contextGuidPtr = this.sqlite3Provider.Sqlite3AggregateContext(context, 0);

                if (contextGuidPtr != IntPtr.Zero)
                {
                    var contextGuidBytes = new byte[16];
                    this.platformMarshal.Copy(contextGuidPtr, contextGuidBytes, 0, 16);
                    var contextGuid = new Guid(contextGuidBytes);

                    aggregateContextData = this.aggregateContextDataDic[contextGuid];
                }

                try
                {
                    var result = final.Invoke(aggregateContextData);
                    SetResult(context, result);
                }
                catch (Exception ex)
                {
                    SetError(context, ex);
                }
            });

            var aggregateFinalDelegate = this.platformMarshal.ApplyNativeCallingConventionToAggregateFinal(nativeAggregateFinal);
            this.aggregateFinalDelegates[name] = aggregateFinalDelegate;

            var aggregateFinalPtr = this.platformMarshal.MarshalDelegateToNativeFunctionPointer(aggregateFinalDelegate);

            int nameLength;
            var namePtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(name, out nameLength);

            try
            {
                this.sqlite3Provider.Sqlite3CreateAggregate(this.db, namePtr, numberOfArguments, aggregateStepPtr, aggregateFinalPtr);
            }
            finally
            {
                if (namePtr != IntPtr.Zero)
                {
                    this.platformMarshal.CleanUpStringNativeUTF8(namePtr);
                }
            }
        }

        public void CreateCollation(string name, Collation collation)
        {
            name = name.ToUpperInvariant();

            var nativeCollation = new CollationNative((applicationData, firstLength, firstString, secondLength, secondString) =>
            {
                var first = this.platformMarshal.MarshalStringNativeUTF8ToManaged(firstString);
                var second = this.platformMarshal.MarshalStringNativeUTF8ToManaged(secondString);

                try
                {
                    return collation.Invoke(first, second);
                }
                catch
                {
                    return 0;
                }
            });

            var collationDelegate = this.platformMarshal.ApplyNativeCallingConventionToCollation(nativeCollation);
            this.collationDelegates[name] = collationDelegate;

            var collPtr = this.platformMarshal.MarshalDelegateToNativeFunctionPointer(collationDelegate);

            int nameLength;
            var namePtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(name, out nameLength);

            try
            {
                this.sqlite3Provider.Sqlite3CreateCollation(this.db, namePtr, collPtr);
            }
            finally
            {
                if (namePtr != IntPtr.Zero)
                {
                    this.platformMarshal.CleanUpStringNativeUTF8(namePtr);
                }
            }
        }

        public long LastInsertRowId()
        {
            try
            {
                var lastId = this.sqlite3Provider.Sqlite3LastInsertRowId(this.db);

                return lastId;
            }
            catch (SQLiteException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SQLiteException("Unable to retrieve the last inserted row id.", ex);
            }
        }

        public int ChangesCount()
        {
            try
            {
                return this.sqlite3Provider.Sqlite3Changes(this.db);
            }
            catch (SQLiteException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SQLiteException("Unable to retrieve the changes count.", ex);
            }
        }

        public string ErrorMessage()
        {
            try
            {
                var errmsgPtr = this.sqlite3Provider.Sqlite3Errmsg(this.db);

                return this.platformMarshal.MarshalStringNativeUTF8ToManaged(errmsgPtr);
            }
            catch (SQLiteException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SQLiteException("Unable to retrieve the error message.", ex);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                //// if (disposing)
                //// {
                ////     // Managed
                //// }

                // Unmanaged
                this.sqlite3Provider.Sqlite3CloseV2(this.db);

                this.db = IntPtr.Zero;

                this.disposed = true;
            }
        }

        private object[] ObtainManagedArguments(IntPtr[] nativeArguments)
        {
            object[] mangedArguments = null;

            if (nativeArguments != null)
            {
                mangedArguments = new object[nativeArguments.Length];

                for (var index = 0; index < nativeArguments.Length; index++)
                {
                    var nativeArgument = nativeArguments[index];
                    object mangedArgument = null;

                    var type = (SQLiteType)this.sqlite3Provider.Sqlite3ValueType(nativeArgument);

                    switch (type)
                    {
                        case SQLiteType.INTEGER:
                            mangedArgument = this.sqlite3Provider.Sqlite3ValueInt64(nativeArgument);
                            break;
                        case SQLiteType.FLOAT:
                            mangedArgument = this.sqlite3Provider.Sqlite3ValueDouble(nativeArgument);
                            break;
                        case SQLiteType.TEXT:
                            var textPointer = this.sqlite3Provider.Sqlite3ValueText(nativeArgument);
                            mangedArgument = this.platformMarshal.MarshalStringNativeUTF8ToManaged(textPointer);
                            break;
                        case SQLiteType.BLOB:
                            var blobPointer = this.sqlite3Provider.Sqlite3ValueBlob(nativeArgument);
                            var length = this.sqlite3Provider.Sqlite3ValueBytes(nativeArgument);
                            mangedArgument = new byte[length];
                            this.platformMarshal.Copy(blobPointer, (byte[])mangedArgument, 0, length);
                            break;
                        case SQLiteType.NULL:
                            break;
                    }

                    mangedArguments[index] = mangedArgument;
                }
            }

            return mangedArguments;
        }

        private void SetResult(IntPtr context, object result)
        {
            if (result == null)
            {
                this.sqlite3Provider.Sqlite3ResultNull(context);
            }
            else
            {
                if (result is int)
                {
                    this.sqlite3Provider.Sqlite3ResultInt(context, (int)result);
                }
                else if (result is long)
                {
                    this.sqlite3Provider.Sqlite3ResultInt64(context, (long)result);
                }
                else if (result is double)
                {
                    this.sqlite3Provider.Sqlite3ResultDouble(context, (double)result);
                }
                else if (result is string)
                {
                    int valueLength;
                    var valuePtr = this.platformMarshal.MarshalStringManagedToNativeUTF8((string)result, out valueLength);

                    try
                    {
                        this.sqlite3Provider.Sqlite3ResultText(context, valuePtr, valueLength - 1, (IntPtr)(-1));
                    }
                    finally
                    {
                        if (valuePtr != IntPtr.Zero)
                        {
                            this.platformMarshal.CleanUpStringNativeUTF8(valuePtr);
                        }
                    }
                }
                else if (result is byte[])
                {
                    this.sqlite3Provider.Sqlite3ResultBlob(context, (byte[])result, ((byte[])result).Length, (IntPtr)(-1));
                }
            }
        }

        private void SetError(IntPtr context, Exception ex)
        {
            int errorLength;
            var errorPtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(ex.Message, out errorLength);

            try
            {
                this.sqlite3Provider.Sqlite3ResultError(context, errorPtr, errorLength - 1);
            }
            finally
            {
                if (errorPtr != IntPtr.Zero)
                {
                    this.platformMarshal.CleanUpStringNativeUTF8(errorPtr);
                }
            }
        }

        private void SetTemporaryDirectory()
        {
            lock (SQLiteConnection.syncTDS)
            {
                if (!SQLiteConnection.temporaryDirectorySet)
                {
                    try
                    {
                        if (this.sqlite3Provider.Sqlite3Win32SetDirectory() != (int)SQLiteResult.OK)
                        {
                            throw new SQLiteException("Unable to set temporary directory.");
                        }

                        SQLiteConnection.temporaryDirectorySet = true;
                    }
                    catch (SQLiteException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new SQLiteException("Unable to set temporary directory.", ex);
                    }
                }
            }
        }
    }
}
