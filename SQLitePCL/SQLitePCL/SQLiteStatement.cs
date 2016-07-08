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
    using System.Runtime.InteropServices;
    using System.Text;

    public class SQLiteStatement : ISQLiteStatement
    {
        private IPlatformMarshal platformMarshal;

        private ISQLite3Provider sqlite3Provider;

        private SQLiteConnection connection;

        private IntPtr stm;

        private Dictionary<string, int> columnNameIndexDic;

        private Dictionary<int, string> columnIndexNameDic;

        private bool disposed = false;

        internal SQLiteStatement(SQLiteConnection connection, IntPtr stm)
        {
            this.platformMarshal = Platform.Instance.PlatformMarshal;
            this.sqlite3Provider = Platform.Instance.SQLite3Provider;

            this.connection = connection;
            this.stm = stm;

            this.columnNameIndexDic = new Dictionary<string, int>();
            this.columnIndexNameDic = new Dictionary<int, string>();

            for (int index = 0; index < this.ColumnCount; index++)
            {
                var name = this.platformMarshal.MarshalStringNativeUTF8ToManaged(this.sqlite3Provider.Sqlite3ColumnName(this.stm, index));

                // Will only track the first appearence of a particular column name
                if (!string.IsNullOrEmpty(name) && !this.columnNameIndexDic.ContainsKey(name))
                {
                    this.columnNameIndexDic.Add(name, index);
                }

                this.columnIndexNameDic.Add(index, name);
            }
        }

        ~SQLiteStatement()
        {
            this.Dispose(false);
        }

        public ISQLiteConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        public int ColumnCount
        {
            get
            {
                return this.sqlite3Provider.Sqlite3ColumnCount(this.stm);
            }
        }

        public int DataCount
        {
            get
            {
                return this.sqlite3Provider.Sqlite3DataCount(this.stm);
            }
        }

        public object this[int index]
        {
            get
            {
                object result = null;

                var type = (SQLiteType)this.sqlite3Provider.Sqlite3ColumnType(this.stm, index);

                switch (type)
                {
                    case SQLiteType.INTEGER:
                        result = this.sqlite3Provider.Sqlite3ColumnInt64(this.stm, index);
                        break;
                    case SQLiteType.FLOAT:
                        result = this.sqlite3Provider.Sqlite3ColumnDouble(this.stm, index);
                        break;
                    case SQLiteType.TEXT:
                        var textPointer = this.sqlite3Provider.Sqlite3ColumnText(this.stm, index);
                        result = this.platformMarshal.MarshalStringNativeUTF8ToManaged(textPointer);
                        break;
                    case SQLiteType.BLOB:
                        var blobPointer = this.sqlite3Provider.Sqlite3ColumnBlob(this.stm, index);

                        if (blobPointer != IntPtr.Zero)
                        {
                            var length = this.sqlite3Provider.Sqlite3ColumnBytes(this.stm, index);
                            result = new byte[length];
                            this.platformMarshal.Copy(blobPointer, (byte[])result, 0, length);
                        }
                        else
                        {
                            result = new byte[0];
                        }

                        break;
                    case SQLiteType.NULL:
                        break;
                }

                return result;
            }
        }

        public object this[string name]
        {
            get
            {
                return this[this.ColumnIndex(name)];
            }
        }

        public SQLiteType DataType(int index)
        {
            return (SQLiteType)this.sqlite3Provider.Sqlite3ColumnType(this.stm, index);
        }

        public SQLiteType DataType(string name)
        {
            return this.DataType(this.ColumnIndex(name));
        }

        public string ColumnName(int index)
        {
            string name;

            if (this.columnIndexNameDic.TryGetValue(index, out name))
            {
                return name;
            }
            else
            {
                throw new SQLiteException("Unable to find column with the specified index: " + index);
            }
        }

        public int ColumnIndex(string name)
        {
            int index;

            if (this.columnNameIndexDic.TryGetValue(name, out index))
            {
                return index;
            }
            else
            {
                throw new SQLiteException("Unable to find column with the specified name: " + name);
            }
        }

        public SQLiteResult Step()
        {
            return (SQLiteResult)this.sqlite3Provider.Sqlite3Step(this.stm);
        }

        public long GetInteger(int index)
        {
            var dataType = this.DataType(index);

            if (dataType != SQLiteType.INTEGER)
            {
                throw new SQLiteException("Unable to cast existing data type to Integer type: " + dataType.ToString());
            }
            else
            {
                return (long)this[index];
            }
        }

        public long GetInteger(string name)
        {
            return GetInteger(this.ColumnIndex(name));
        }

        public double GetFloat(int index)
        {
            var dataType = this.DataType(index);

            if (dataType != SQLiteType.FLOAT)
            {
                throw new SQLiteException("Unable to cast existing data type to Float type: " + dataType.ToString());
            }
            else
            {
                return (double)this[index];
            }
        }

        public double GetFloat(string name)
        {
            return this.GetFloat(this.ColumnIndex(name));
        }

        public string GetText(int index)
        {
            var dataType = this.DataType(index);

            if (dataType != SQLiteType.TEXT)
            {
                throw new SQLiteException("Unable to cast existing data type to Text type: " + dataType.ToString());
            }
            else
            {
                return (string)this[index];
            }
        }

        public string GetText(string name)
        {
            return this.GetText(this.ColumnIndex(name));
        }

        public byte[] GetBlob(int index)
        {
            var dataType = this.DataType(index);

            if (dataType != SQLiteType.BLOB)
            {
                throw new SQLiteException("Unable to cast existing data type to Blob type: " + dataType.ToString());
            }
            else
            {
                return (byte[])this[index];
            }
        }

        public byte[] GetBlob(string name)
        {
            return this.GetBlob(this.ColumnIndex(name));
        }

        public void Reset()
        {
            if (this.sqlite3Provider.Sqlite3Reset(this.stm) != (int)SQLiteResult.OK)
            {
                var errmsg = this.connection.ErrorMessage();
                throw new SQLiteException(errmsg);
            }
        }

        public void Bind(int index, object value)
        {
            var invokeResult = 0;

            if (value == null)
            {
                invokeResult = this.sqlite3Provider.Sqlite3BindNull(this.stm, index);
            }
            else
            {
                if (IsSupportedInteger(value))
                {
                    invokeResult = this.sqlite3Provider.Sqlite3BindInt64(this.stm, index, GetInteger(value));
                }
                else if (IsSupportedFloat(value))
                {
                    invokeResult = this.sqlite3Provider.Sqlite3BindDouble(this.stm, index, GetFloat(value));
                }
                else if (IsSupportedText(value))
                {
                    int valueLength;
                    var valuePtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(value.ToString(), out valueLength);

                    try
                    {
                        invokeResult = this.sqlite3Provider.Sqlite3BindText(this.stm, index, valuePtr, valueLength - 1, (IntPtr)(-1));
                    }
                    finally
                    {
                        if (valuePtr != IntPtr.Zero)
                        {
                            this.platformMarshal.CleanUpStringNativeUTF8(valuePtr);
                        }
                    }
                }
                else if (value is byte[])
                {
                    invokeResult = this.sqlite3Provider.Sqlite3BindBlob(this.stm, index, (byte[])value, ((byte[])value).Length, (IntPtr)(-1));
                }
                else
                {
                    throw new SQLiteException("Unable to bind parameter with unsupported type: " + value.GetType().FullName);
                }
            }

            if (invokeResult != (int)SQLiteResult.OK)
            {
                var errmsg = this.connection.ErrorMessage();
                throw new SQLiteException(errmsg);
            }
        }

        public void Bind(string paramName, object value)
        {
            var paramNamePtr = this.platformMarshal.MarshalStringManagedToNativeUTF8(paramName);

            try
            {
                var index = this.sqlite3Provider.Sqlite3BindParameterIndex(this.stm, paramNamePtr);
                this.Bind(index, value);
            }
            finally
            {
                if (paramNamePtr != IntPtr.Zero)
                {
                    this.platformMarshal.CleanUpStringNativeUTF8(paramNamePtr);
                }
            }
        }

        public void ClearBindings()
        {
            if (this.sqlite3Provider.Sqlite3ClearBindings(this.stm) != (int)SQLiteResult.OK)
            {
                var errmsg = this.connection.ErrorMessage();
                throw new SQLiteException(errmsg);
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
                this.sqlite3Provider.Sqlite3Finalize(this.stm);

                this.stm = IntPtr.Zero;

                this.disposed = true;
            }
        }

        private static bool IsSupportedInteger(object value)
        {
            return value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong;
        }

        private static bool IsSupportedFloat(object value)
        {
            return value is decimal || value is float || value is double;
        }

        private static bool IsSupportedText(object value)
        {
            return value is char || value is string;
        }

        private static long GetInteger(object value)
        {
            var longValue = 0L;

            if (value is byte)
            {
                longValue = (long)(byte)value;
            }
            else if (value is sbyte)
            {
                longValue = (long)(sbyte)value;
            }
            else if (value is short)
            {
                longValue = (long)(short)value;
            }
            else if (value is ushort)
            {
                longValue = (long)(ushort)value;
            }
            else if (value is int)
            {
                longValue = (long)(int)value;
            }
            else if (value is uint)
            {
                longValue = (long)(uint)value;
            }
            else if (value is long)
            {
                longValue = (long)value;
            }
            else if (value is ulong)
            {
                if ((ulong)value > long.MaxValue)
                {
                    throw new SQLiteException("Unable to cast provided ulong value. Overflow ocurred: " + value.ToString());
                }

                longValue = (long)(ulong)value;
            }
            else
            {
                throw new SQLiteException("Unable to cast provided value with unsupported Integer type: " + value.GetType().FullName);
            }

            return longValue;
        }

        private static double GetFloat(object value)
        {
            var doubleValue = 0d;

            if (value is decimal)
            {
                doubleValue = (double)(decimal)value;
            }
            else if (value is float)
            {
                doubleValue = (double)(float)value;
            }
            else if (value is double)
            {
                doubleValue = (double)value;
            }
            else
            {
                throw new SQLiteException("Unable to cast provided value with unsupported Real type: " + value.GetType().FullName);
            }

            return doubleValue;
        }
    }
}
