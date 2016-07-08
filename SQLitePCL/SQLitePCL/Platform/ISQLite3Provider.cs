// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.

namespace SQLitePCL
{
    using System;

    /// <summary>
    /// Provides core functionality of the SQLite3 API.
    /// </summary>
    public interface ISQLite3Provider
    {
        int Sqlite3Win32SetDirectory();

        int Sqlite3Open(IntPtr filename, out IntPtr db, int flags);

        int Sqlite3CloseV2(IntPtr db);

        int Sqlite3PrepareV2(IntPtr db, IntPtr sql, int length, out IntPtr stm, IntPtr tail);

        int Sqlite3CreateFunction(IntPtr db, IntPtr functionName, int numArg, bool deterministic, IntPtr func);

        int Sqlite3CreateAggregate(IntPtr db, IntPtr aggregateName, int numArg, IntPtr step, IntPtr final);

        int Sqlite3CreateCollation(IntPtr db, IntPtr collationName, IntPtr compare);

        long Sqlite3LastInsertRowId(IntPtr context);

        IntPtr Sqlite3Errmsg(IntPtr db);

        int Sqlite3BindInt(IntPtr stm, int paramIndex, int value);

        int Sqlite3BindInt64(IntPtr stm, int paramIndex, long value);

        int Sqlite3BindText(IntPtr stm, int paramIndex, IntPtr value, int length, IntPtr destructor);

        int Sqlite3BindDouble(IntPtr stm, int paramIndex, double value);

        int Sqlite3BindBlob(IntPtr stm, int paramIndex, byte[] value, int length, IntPtr destructor);

        int Sqlite3BindNull(IntPtr stm, int paramIndex);

        int Sqlite3BindParameterCount(IntPtr stm);

        IntPtr Sqlite3BindParameterName(IntPtr stm, int paramIndex);

        int Sqlite3BindParameterIndex(IntPtr stm, IntPtr paramName);

        int Sqlite3Step(IntPtr stm);

        int Sqlite3ColumnInt(IntPtr stm, int columnIndex);

        long Sqlite3ColumnInt64(IntPtr stm, int columnIndex);

        IntPtr Sqlite3ColumnText(IntPtr stm, int columnIndex);

        double Sqlite3ColumnDouble(IntPtr stm, int columnIndex);

        IntPtr Sqlite3ColumnBlob(IntPtr stm, int columnIndex);

        int Sqlite3ColumnType(IntPtr stm, int columnIndex);

        int Sqlite3ColumnBytes(IntPtr stm, int columnIndex);

        int Sqlite3ColumnCount(IntPtr stm);

        IntPtr Sqlite3ColumnName(IntPtr stm, int columnIndex);

        IntPtr Sqlite3ColumnOriginName(IntPtr stm, int columnIndex);

        IntPtr Sqlite3ColumnTableName(IntPtr stm, int columnIndex);

        IntPtr Sqlite3ColumnDatabaseName(IntPtr stm, int columnIndex);

        int Sqlite3DataCount(IntPtr stm);

        int Sqlite3Reset(IntPtr stm);

        int Sqlite3ClearBindings(IntPtr stm);

        int Sqlite3Finalize(IntPtr stm);

        int Sqlite3ValueInt(IntPtr value);

        long Sqlite3ValueInt64(IntPtr value);

        IntPtr Sqlite3ValueText(IntPtr value);

        double Sqlite3ValueDouble(IntPtr value);

        IntPtr Sqlite3ValueBlob(IntPtr value);

        int Sqlite3ValueType(IntPtr value);

        int Sqlite3ValueBytes(IntPtr value);

        void Sqlite3ResultInt(IntPtr context, int value);

        void Sqlite3ResultInt64(IntPtr context, long value);

        void Sqlite3ResultText(IntPtr context, IntPtr value, int length, IntPtr destructor);

        void Sqlite3ResultDouble(IntPtr context, double value);

        void Sqlite3ResultBlob(IntPtr context, byte[] value, int length, IntPtr destructor);

        void Sqlite3ResultNull(IntPtr context);

        void Sqlite3ResultError(IntPtr context, IntPtr value, int length);

        IntPtr Sqlite3AggregateContext(IntPtr context, int length);

        int Sqlite3Changes(IntPtr db);
    }
}