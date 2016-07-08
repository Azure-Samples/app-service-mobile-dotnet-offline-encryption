﻿// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.

namespace SQLitePCL
{
    /// <summary>
    /// Provides an interface that platform-specific SQLite Wrapper assemblies 
    /// can implement to provide functionality required by the SQLite Wrapper PCL 
    /// that is platform specific.
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// Returns a platform-specific implemention of <see cref="IPlatformMarshal"/>.
        /// </summary>
        IPlatformMarshal PlatformMarshal { get; }

        /// <summary>
        /// Returns a platform-specific implemention of <see cref="IPlatformStorage"/>.
        /// </summary>
        IPlatformStorage PlatformStorage { get; }

        /// <summary>
        /// Returns a platform-specific implemention of <see cref="ISQLite3Provider"/>.
        /// </summary>
        ISQLite3Provider SQLite3Provider { get; }
    }
}
