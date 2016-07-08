// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.
// Note: This is modified version of https://sqlitepcl.codeplex.com/SourceControl/latest#src/SQLitePCL/SQLitePCL.Ext.iOS.UnifiedAPI/Platform/SQLite3Provider.cs

namespace SQLitePCL
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using ObjCRuntime;

    /// <summary>
    /// Implements the <see cref="ISQLite3Provider"/> interface for Xamarin iOS.
    /// </summary>
    internal sealed class SQLite3Provider : ISQLite3Provider
    {
        /// <summary>
        /// A singleton instance of the <see cref="SQLite3Provider"/>.
        /// </summary>
        private static ISQLite3Provider instance = new SQLite3Provider();

        private SQLite3Provider()
        {
        }

        /// <summary>
        /// A singleton instance of the <see cref="SQLite3Provider"/>.
        /// </summary>
        internal static ISQLite3Provider Instance
        {
            get
            {
                return instance;
            }
        }

        int ISQLite3Provider.Sqlite3Win32SetDirectory()
        {
            return 0;
        }

        int ISQLite3Provider.Sqlite3Open(IntPtr filename, out IntPtr db, int flags)
        {
            return NativeMethods.sqlite3_open_v2(filename, out db, flags, IntPtr.Zero);
        }

        int ISQLite3Provider.Sqlite3CloseV2(IntPtr db)
        {
            Sqlite3FunctionMarshallingProxy.ReleaseProxies(db);
            return NativeMethods.sqlite3_close_v2(db);
        }

        int ISQLite3Provider.Sqlite3PrepareV2(IntPtr db, IntPtr sql, int length, out IntPtr stm, IntPtr tail)
        {
            return NativeMethods.sqlite3_prepare_v2(db, sql, length, out stm, tail);
        }

        int ISQLite3Provider.Sqlite3CreateFunction(IntPtr db, IntPtr functionName, int numArg, bool deterministic, IntPtr func)
        {
            var proxyFunction = Marshal.GetFunctionPointerForDelegate(new FunctionNativeCdecl(Sqlite3FunctionMarshallingProxy.FunctionNativeCdeclProxy));

            var userData = new Sqlite3FunctionMarshallingProxy(db, functionName, func).GuidHandlePtr;

            return NativeMethods.sqlite3_create_function(db, functionName, numArg, deterministic ? 0x801 : 1, userData, proxyFunction, IntPtr.Zero, IntPtr.Zero);
        }

        int ISQLite3Provider.Sqlite3CreateAggregate(IntPtr db, IntPtr aggregateName, int numArg, IntPtr step, IntPtr final)
        {
            var proxyStep = Marshal.GetFunctionPointerForDelegate(new AggregateStepNativeCdecl(Sqlite3FunctionMarshallingProxy.AggregateStepNativeCdeclProxy));
            var proxyFinal = Marshal.GetFunctionPointerForDelegate(new AggregateFinalNativeCdecl(Sqlite3FunctionMarshallingProxy.AggregateFinalNativeCdeclProxy));

            var userData = new Sqlite3FunctionMarshallingProxy(db, aggregateName, step, final).GuidHandlePtr;

            return NativeMethods.sqlite3_create_function(db, aggregateName, numArg, 1, userData, IntPtr.Zero, proxyStep, proxyFinal);
        }

        int ISQLite3Provider.Sqlite3CreateCollation(IntPtr db, IntPtr collationName, IntPtr compare)
        {
            var proxyCollation = Marshal.GetFunctionPointerForDelegate(new CollationNativeCdecl(Sqlite3CollationMarshallingProxy.CollationNativeCdeclProxy));

            var applicationData = new Sqlite3CollationMarshallingProxy(db, collationName, compare).GuidHandlePtr;

            return NativeMethods.sqlite3_create_collation(db, collationName, 0x1, applicationData, proxyCollation);
        }

        long ISQLite3Provider.Sqlite3LastInsertRowId(IntPtr db)
        {
            return NativeMethods.sqlite3_last_insert_rowid(db);
        }

        IntPtr ISQLite3Provider.Sqlite3Errmsg(IntPtr db)
        {
            return NativeMethods.sqlite3_errmsg(db);
        }

        int ISQLite3Provider.Sqlite3BindInt(IntPtr stm, int paramIndex, int value)
        {
            return NativeMethods.sqlite3_bind_int(stm, paramIndex, value);
        }

        int ISQLite3Provider.Sqlite3BindInt64(IntPtr stm, int paramIndex, long value)
        {
            return NativeMethods.sqlite3_bind_int64(stm, paramIndex, value);
        }

        int ISQLite3Provider.Sqlite3BindText(IntPtr stm, int paramIndex, IntPtr value, int length, IntPtr destructor)
        {
            return NativeMethods.sqlite3_bind_text(stm, paramIndex, value, length, destructor);
        }

        int ISQLite3Provider.Sqlite3BindDouble(IntPtr stm, int paramIndex, double value)
        {
            return NativeMethods.sqlite3_bind_double(stm, paramIndex, value);
        }

        int ISQLite3Provider.Sqlite3BindBlob(IntPtr stm, int paramIndex, byte[] value, int length, IntPtr destructor)
        {
            return NativeMethods.sqlite3_bind_blob(stm, paramIndex, value, length, destructor);
        }

        int ISQLite3Provider.Sqlite3BindNull(IntPtr stm, int paramIndex)
        {
            return NativeMethods.sqlite3_bind_null(stm, paramIndex);
        }

        int ISQLite3Provider.Sqlite3BindParameterCount(IntPtr stm)
        {
            return NativeMethods.sqlite3_bind_parameter_count(stm);
        }

        IntPtr ISQLite3Provider.Sqlite3BindParameterName(IntPtr stm, int paramIndex)
        {
            return NativeMethods.sqlite3_bind_parameter_name(stm, paramIndex);
        }

        int ISQLite3Provider.Sqlite3BindParameterIndex(IntPtr stm, IntPtr paramName)
        {
            return NativeMethods.sqlite3_bind_parameter_index(stm, paramName);
        }

        int ISQLite3Provider.Sqlite3Step(IntPtr stm)
        {
            return NativeMethods.sqlite3_step(stm);
        }

        int ISQLite3Provider.Sqlite3ColumnInt(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_int(stm, columnIndex);
        }

        long ISQLite3Provider.Sqlite3ColumnInt64(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_int64(stm, columnIndex);
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnText(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_text(stm, columnIndex);
        }

        double ISQLite3Provider.Sqlite3ColumnDouble(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_double(stm, columnIndex);
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnBlob(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_blob(stm, columnIndex);
        }

        int ISQLite3Provider.Sqlite3ColumnType(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_type(stm, columnIndex);
        }

        int ISQLite3Provider.Sqlite3ColumnBytes(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_bytes(stm, columnIndex);
        }

        int ISQLite3Provider.Sqlite3ColumnCount(IntPtr stm)
        {
            return NativeMethods.sqlite3_column_count(stm);
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnName(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_name(stm, columnIndex);
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnOriginName(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_origin_name(stm, columnIndex);
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnTableName(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_table_name(stm, columnIndex);
        }

        IntPtr ISQLite3Provider.Sqlite3ColumnDatabaseName(IntPtr stm, int columnIndex)
        {
            return NativeMethods.sqlite3_column_database_name(stm, columnIndex);
        }

        int ISQLite3Provider.Sqlite3DataCount(IntPtr stm)
        {
            return NativeMethods.sqlite3_data_count(stm);
        }

        int ISQLite3Provider.Sqlite3Reset(IntPtr stm)
        {
            return NativeMethods.sqlite3_reset(stm);
        }

        int ISQLite3Provider.Sqlite3ClearBindings(IntPtr stm)
        {
            return NativeMethods.sqlite3_clear_bindings(stm);
        }

        int ISQLite3Provider.Sqlite3Finalize(IntPtr stm)
        {
            return NativeMethods.sqlite3_finalize(stm);
        }

        int ISQLite3Provider.Sqlite3ValueInt(IntPtr value)
        {
            return NativeMethods.sqlite3_value_int(value);
        }

        long ISQLite3Provider.Sqlite3ValueInt64(IntPtr value)
        {
            return NativeMethods.sqlite3_value_int64(value);
        }

        IntPtr ISQLite3Provider.Sqlite3ValueText(IntPtr value)
        {
            return NativeMethods.sqlite3_value_text(value);
        }

        double ISQLite3Provider.Sqlite3ValueDouble(IntPtr value)
        {
            return NativeMethods.sqlite3_value_double(value);
        }

        IntPtr ISQLite3Provider.Sqlite3ValueBlob(IntPtr value)
        {
            return NativeMethods.sqlite3_value_blob(value);
        }

        int ISQLite3Provider.Sqlite3ValueType(IntPtr value)
        {
            return NativeMethods.sqlite3_value_type(value);
        }

        int ISQLite3Provider.Sqlite3ValueBytes(IntPtr value)
        {
            return NativeMethods.sqlite3_value_bytes(value);
        }

        void ISQLite3Provider.Sqlite3ResultInt(IntPtr context, int value)
        {
            NativeMethods.sqlite3_result_int(context, value);
        }

        void ISQLite3Provider.Sqlite3ResultInt64(IntPtr context, long value)
        {
            NativeMethods.sqlite3_result_int64(context, value);
        }

        void ISQLite3Provider.Sqlite3ResultText(IntPtr context, IntPtr value, int length, IntPtr destructor)
        {
            NativeMethods.sqlite3_result_text(context, value, length, destructor);
        }

        void ISQLite3Provider.Sqlite3ResultDouble(IntPtr context, double value)
        {
            NativeMethods.sqlite3_result_double(context, value);
        }

        void ISQLite3Provider.Sqlite3ResultBlob(IntPtr context, byte[] value, int length, IntPtr destructor)
        {
            NativeMethods.sqlite3_result_blob(context, value, length, destructor);
        }

        void ISQLite3Provider.Sqlite3ResultNull(IntPtr context)
        {
            NativeMethods.sqlite3_result_null(context);
        }

        void ISQLite3Provider.Sqlite3ResultError(IntPtr context, IntPtr value, int length)
        {
            NativeMethods.sqlite3_result_error(context, value, length);
        }

        IntPtr ISQLite3Provider.Sqlite3AggregateContext(IntPtr context, int length)
        {
            return NativeMethods.sqlite3_aggregate_context(context, length);
        }

        int ISQLite3Provider.Sqlite3Changes(IntPtr db)
        {
            return NativeMethods.sqlite3_changes(db);
        }

        // unwrap the result of MarshalDelegateToNativeFunctionPointer as we are
        // not going to be using this pointer, but rather the pointer to this 
        // object, which will contain the references to the function that the 
        // current pointer points to.
        private struct Sqlite3FunctionMarshallingProxy
        {
            private static IDictionary<Guid, Sqlite3FunctionMarshallingProxy> instanceByGuidDic = new Dictionary<Guid, Sqlite3FunctionMarshallingProxy>();

            private static IDictionary<long, IDictionary<string, Guid>> guidByDbFuncDic = new Dictionary<long, IDictionary<string, Guid>>();

            // functions
            private readonly FunctionNativeCdecl invoke;

            // aggregates   
            private readonly AggregateStepNativeCdecl step;
            private readonly AggregateFinalNativeCdecl final;

            private readonly Guid guid;
            private readonly GCHandle guidHandle;

            internal Sqlite3FunctionMarshallingProxy(IntPtr db, IntPtr functionName, IntPtr invoke)
            {
                var invokeFunction = GCHandle.FromIntPtr(invoke);
                this.invoke = invokeFunction.Target as FunctionNativeCdecl;
                invokeFunction.Free();

                this.step = null;

                this.final = null;

                this.guid = Guid.NewGuid();
                this.guidHandle = GCHandle.Alloc(this.guid, GCHandleType.Pinned);

                this.RegisterInstance(db, functionName);
            }

            internal Sqlite3FunctionMarshallingProxy(IntPtr db, IntPtr aggregateName, IntPtr step, IntPtr final)
            {
                this.invoke = null;

                var stepFunction = GCHandle.FromIntPtr(step);
                this.step = stepFunction.Target as AggregateStepNativeCdecl;
                stepFunction.Free();

                var finalFunction = GCHandle.FromIntPtr(final);
                this.final = finalFunction.Target as AggregateFinalNativeCdecl;
                finalFunction.Free();

                this.guid = Guid.NewGuid();
                this.guidHandle = GCHandle.Alloc(this.guid, GCHandleType.Pinned);

                this.RegisterInstance(db, aggregateName);
            }

            internal IntPtr GuidHandlePtr
            {
                get
                {
                    return GCHandle.ToIntPtr(this.guidHandle);
                }
            }

            [MonoPInvokeCallback(typeof(FunctionNativeCdecl))]
            internal static void FunctionNativeCdeclProxy(IntPtr context, int numberOfArguments, IntPtr[] arguments)
            {
                // Fetch the "pApp" value that was passed to sqlite3_create_function
                var guidHandlePtr = NativeMethods.sqlite3_user_data(context);
                var guidHandle = GCHandle.FromIntPtr(guidHandlePtr);
                var proxy = Sqlite3FunctionMarshallingProxy.instanceByGuidDic[(Guid)guidHandle.Target];

                // Invoke it
                proxy.invoke(context, numberOfArguments, arguments);
            }

            [MonoPInvokeCallback(typeof(AggregateStepNativeCdecl))]
            internal static void AggregateStepNativeCdeclProxy(IntPtr context, int numberOfArguments, IntPtr[] arguments)
            {
                // Fetch the "pApp" value that was passed to sqlite3_create_function
                var guidHandlePtr = NativeMethods.sqlite3_user_data(context);
                var guidHandle = GCHandle.FromIntPtr(guidHandlePtr);
                var proxy = Sqlite3FunctionMarshallingProxy.instanceByGuidDic[(Guid)guidHandle.Target];

                // Invoke the STEP function
                proxy.step(context, numberOfArguments, arguments);
            }

            [MonoPInvokeCallback(typeof(AggregateFinalNativeCdecl))]
            internal static void AggregateFinalNativeCdeclProxy(IntPtr context)
            {
                // Fetch the "pApp" value that was passed to sqlite3_create_function
                var guidHandlePtr = NativeMethods.sqlite3_user_data(context);
                var guidHandle = GCHandle.FromIntPtr(guidHandlePtr);
                var proxy = Sqlite3FunctionMarshallingProxy.instanceByGuidDic[(Guid)guidHandle.Target];

                // Invoke the FINAL function
                proxy.final(context);
            }

            internal static void ReleaseProxies(IntPtr db)
            {
                IDictionary<string, Guid> guidByFunc;

                if (guidByDbFuncDic.TryGetValue(db.ToInt64(), out guidByFunc))
                {
                    foreach (var oldGuid in guidByFunc.Values)
                    {
                        instanceByGuidDic[oldGuid].guidHandle.Free();
                        instanceByGuidDic.Remove(oldGuid);
                    }

                    guidByDbFuncDic.Remove(db.ToInt64());
                }
            }

            private void RegisterInstance(IntPtr db, IntPtr functionName)
            {
                var funcName = PlatformMarshal.Instance.MarshalStringNativeUTF8ToManaged(functionName);

                IDictionary<string, Guid> guidByFunc;

                if (guidByDbFuncDic.TryGetValue(db.ToInt64(), out guidByFunc))
                {
                    Guid oldGuid;

                    if (guidByFunc.TryGetValue(funcName, out oldGuid))
                    {
                        instanceByGuidDic[oldGuid].guidHandle.Free();
                        instanceByGuidDic.Remove(oldGuid);
                        guidByFunc[funcName] = this.guid;
                    }
                    else
                    {
                        guidByFunc.Add(funcName, this.guid);
                    }
                }
                else
                {
                    guidByFunc = new Dictionary<string, Guid>();
                    guidByFunc.Add(funcName, this.guid);
                    guidByDbFuncDic.Add(db.ToInt64(), guidByFunc);
                }

                instanceByGuidDic.Add(this.guid, this);
            }
        }

        private struct Sqlite3CollationMarshallingProxy
        {
            private static IDictionary<Guid, Sqlite3CollationMarshallingProxy> instanceByGuidDic = new Dictionary<Guid, Sqlite3CollationMarshallingProxy>();

            private static IDictionary<long, IDictionary<string, Guid>> guidByDbCollDic = new Dictionary<long, IDictionary<string, Guid>>();

            // collations
            private readonly CollationNativeCdecl invoke;

            private readonly Guid guid;
            private readonly GCHandle guidHandle;

            internal Sqlite3CollationMarshallingProxy(IntPtr db, IntPtr collationName, IntPtr invoke)
            {
                var invokeCollation = GCHandle.FromIntPtr(invoke);
                this.invoke = invokeCollation.Target as CollationNativeCdecl;
                invokeCollation.Free();

                this.guid = Guid.NewGuid();
                this.guidHandle = GCHandle.Alloc(this.guid, GCHandleType.Pinned);

                this.RegisterInstance(db, collationName);
            }

            internal IntPtr GuidHandlePtr
            {
                get
                {
                    return GCHandle.ToIntPtr(this.guidHandle);
                }
            }

            [MonoPInvokeCallback(typeof(CollationNativeCdecl))]
            internal static int CollationNativeCdeclProxy(IntPtr applicationData, int firstLength, IntPtr firstString, int secondLength, IntPtr secondString)
            {
                var guidHandle = GCHandle.FromIntPtr(applicationData);
                var proxy = Sqlite3CollationMarshallingProxy.instanceByGuidDic[(Guid)guidHandle.Target];

                // Invoke it
                return proxy.invoke(applicationData, firstLength, firstString, secondLength, secondString);
            }

            internal static void ReleaseProxies(IntPtr db)
            {
                IDictionary<string, Guid> guidByColl;

                if (guidByDbCollDic.TryGetValue(db.ToInt64(), out guidByColl))
                {
                    foreach (var oldGuid in guidByColl.Values)
                    {
                        instanceByGuidDic[oldGuid].guidHandle.Free();
                        instanceByGuidDic.Remove(oldGuid);
                    }

                    guidByDbCollDic.Remove(db.ToInt64());
                }
            }

            private void RegisterInstance(IntPtr db, IntPtr collationName)
            {
                var collName = PlatformMarshal.Instance.MarshalStringNativeUTF8ToManaged(collationName);

                IDictionary<string, Guid> guidByColl;

                if (guidByDbCollDic.TryGetValue(db.ToInt64(), out guidByColl))
                {
                    Guid oldGuid;

                    if (guidByColl.TryGetValue(collName, out oldGuid))
                    {
                        instanceByGuidDic[oldGuid].guidHandle.Free();
                        instanceByGuidDic.Remove(oldGuid);
                        guidByColl[collName] = this.guid;
                    }
                    else
                    {
                        guidByColl.Add(collName, this.guid);
                    }
                }
                else
                {
                    guidByColl = new Dictionary<string, Guid>();
                    guidByColl.Add(collName, this.guid);
                    guidByDbCollDic.Add(db.ToInt64(), guidByColl);
                }

                instanceByGuidDic.Add(this.guid, this);
            }
        }

        private static class NativeMethods
        {
            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_open")]
            internal static extern int sqlite3_open(IntPtr filename, out IntPtr db);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_open_v2")]
            internal static extern int sqlite3_open_v2(IntPtr filename, out IntPtr db, int flags, IntPtr zVfs);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_close")]
            internal static extern int sqlite3_close_v2(IntPtr db);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_prepare_v2")]
            internal static extern int sqlite3_prepare_v2(IntPtr db, IntPtr zSql, int nByte, out IntPtr ppStmpt, IntPtr pzTail);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_create_function")]
            internal static extern int sqlite3_create_function(IntPtr db, IntPtr functionName, int nArg, int p, IntPtr intPtr1, IntPtr func, IntPtr intPtr2, IntPtr intPtr3);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_create_collation")]
            internal static extern int sqlite3_create_collation(IntPtr db, IntPtr collationName, int textRep, IntPtr app, IntPtr compare);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_last_insert_rowid")]
            internal static extern long sqlite3_last_insert_rowid(IntPtr db);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_errmsg")]
            internal static extern IntPtr sqlite3_errmsg(IntPtr db);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_user_data")]
            internal static extern IntPtr sqlite3_user_data(IntPtr context);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_int")]
            internal static extern int sqlite3_bind_int(IntPtr stmHandle, int iParam, int value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_int64")]
            internal static extern int sqlite3_bind_int64(IntPtr stmHandle, int iParam, long value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_text")]
            internal static extern int sqlite3_bind_text(IntPtr stmHandle, int iParam, IntPtr value, int length, IntPtr destructor);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_double")]
            internal static extern int sqlite3_bind_double(IntPtr stmHandle, int iParam, double value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_blob")]
            internal static extern int sqlite3_bind_blob(IntPtr stmHandle, int iParam, byte[] value, int length, IntPtr destructor);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_null")]
            internal static extern int sqlite3_bind_null(IntPtr stmHandle, int iParam);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_parameter_count")]
            internal static extern int sqlite3_bind_parameter_count(IntPtr stmHandle);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_parameter_name")]
            internal static extern IntPtr sqlite3_bind_parameter_name(IntPtr stmHandle, int iParam);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_parameter_index")]
            internal static extern int sqlite3_bind_parameter_index(IntPtr stmHandle, IntPtr paramName);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_step")]
            internal static extern int sqlite3_step(IntPtr stmHandle);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_int")]
            internal static extern int sqlite3_column_int(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_int64")]
            internal static extern long sqlite3_column_int64(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_text")]
            internal static extern IntPtr sqlite3_column_text(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_double")]
            internal static extern double sqlite3_column_double(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_blob")]
            internal static extern IntPtr sqlite3_column_blob(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_type")]
            internal static extern int sqlite3_column_type(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_bytes")]
            internal static extern int sqlite3_column_bytes(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_count")]
            internal static extern int sqlite3_column_count(IntPtr stmHandle);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_name")]
            internal static extern IntPtr sqlite3_column_name(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_origin_name")]
            internal static extern IntPtr sqlite3_column_origin_name(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_table_name")]
            internal static extern IntPtr sqlite3_column_table_name(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_database_name")]
            internal static extern IntPtr sqlite3_column_database_name(IntPtr stmHandle, int iCol);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_data_count")]
            internal static extern int sqlite3_data_count(IntPtr stmHandle);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_reset")]
            internal static extern int sqlite3_reset(IntPtr stmHandle);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_clear_bindings")]
            internal static extern int sqlite3_clear_bindings(IntPtr stmHandle);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_finalize")]
            internal static extern int sqlite3_finalize(IntPtr stmHandle);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_value_int")]
            internal static extern int sqlite3_value_int(IntPtr value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_value_int64")]
            internal static extern long sqlite3_value_int64(IntPtr value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_value_text")]
            internal static extern IntPtr sqlite3_value_text(IntPtr value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_value_double")]
            internal static extern double sqlite3_value_double(IntPtr value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_value_blob")]
            internal static extern IntPtr sqlite3_value_blob(IntPtr value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_value_type")]
            internal static extern int sqlite3_value_type(IntPtr value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_value_bytes")]
            internal static extern int sqlite3_value_bytes(IntPtr value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_result_int")]
            internal static extern void sqlite3_result_int(IntPtr context, int value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_result_int64")]
            internal static extern void sqlite3_result_int64(IntPtr context, long value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_result_text")]
            internal static extern void sqlite3_result_text(IntPtr context, IntPtr value, int length, IntPtr destructor);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_result_double")]
            internal static extern void sqlite3_result_double(IntPtr context, double value);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_result_blob")]
            internal static extern void sqlite3_result_blob(IntPtr context, byte[] value, int length, IntPtr destructor);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_result_null")]
            internal static extern void sqlite3_result_null(IntPtr context);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_result_error")]
            internal static extern void sqlite3_result_error(IntPtr context, IntPtr value, int length);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_aggregate_context")]
            internal static extern IntPtr sqlite3_aggregate_context(IntPtr context, int length);

            [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_changes")]
            internal static extern int sqlite3_changes(IntPtr db);
        }
    }
}