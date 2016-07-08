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

    public interface ISQLiteStatement : IDisposable
    {
        ISQLiteConnection Connection { get; }

        int ColumnCount { get; }

        int DataCount { get; }

        object this[int index] { get; }

        object this[string name] { get; }

        SQLiteType DataType(int index);

        SQLiteType DataType(string name);

        string ColumnName(int index);

        int ColumnIndex(string name);

        SQLiteResult Step();

        long GetInteger(int index);

        long GetInteger(string name);

        double GetFloat(int index);

        double GetFloat(string name);

        string GetText(int index);

        string GetText(string name);

        byte[] GetBlob(int index);

        byte[] GetBlob(string name);

        void Reset();

        void Bind(int index, object value);

        void Bind(string paramName, object value);

        void ClearBindings();
    }
}
