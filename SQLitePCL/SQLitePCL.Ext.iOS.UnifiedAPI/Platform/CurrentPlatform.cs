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
    using Foundation;

    /// <summary>
    /// Implements the <see cref="IPlatform"/> interface for Xamarin iOS.
    /// </summary>
    public sealed class CurrentPlatform : IPlatform
    {
        /// <summary>
        /// Returns a platform-specific implemention of <see cref="IPlatformMarshal"/>.
        /// </summary>
        IPlatformMarshal IPlatform.PlatformMarshal
        {
            get
            {
                return SQLitePCL.PlatformMarshal.Instance;
            }
        }

        /// <summary>
        /// Returns a platform-specific implemention of <see cref="IPlatformStorage"/>.
        /// </summary>
        IPlatformStorage IPlatform.PlatformStorage
        {
            get
            {
                return SQLitePCL.PlatformStorage.Instance;
            }
        }

        /// <summary>
        /// Returns a platform-specific implemention of <see cref="ISQLite3Provider"/>.
        /// </summary>
        ISQLite3Provider IPlatform.SQLite3Provider
        {
            get
            {
                return SQLitePCL.SQLite3Provider.Instance;
            }
        }

        /// <summary>
        /// You must call this method from your application in order to ensure
        /// that this platform specific assembly is included in your app.
        /// </summary>
        public static void Init()
        {
        }
    }
}