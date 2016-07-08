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

    public delegate void FunctionNative(IntPtr context, int numberOfArguments, IntPtr[] arguments);

    public delegate void AggregateStepNative(IntPtr context, int numberOfArguments, IntPtr[] arguments);

    public delegate void AggregateFinalNative(IntPtr context);

    public delegate int CollationNative(IntPtr applicationData, int firstLength, IntPtr firstString, int secondLength, IntPtr secondString);

    /// <summary>
    /// An interface for platform-specific assemblies to implement to support 
    /// Marshaling operations.
    /// </summary>
    public interface IPlatformMarshal
    {
        void CleanUpStringNativeUTF8(IntPtr nativeString);

        IntPtr MarshalStringManagedToNativeUTF8(string managedString);

        IntPtr MarshalStringManagedToNativeUTF8(string managedString, out int size);

        string MarshalStringNativeUTF8ToManaged(IntPtr nativeString);

        int GetNativeUTF8Size(IntPtr nativeString);

        Delegate ApplyNativeCallingConventionToFunction(FunctionNative function);

        Delegate ApplyNativeCallingConventionToAggregateStep(AggregateStepNative step);

        Delegate ApplyNativeCallingConventionToAggregateFinal(AggregateFinalNative final);

        Delegate ApplyNativeCallingConventionToCollation(CollationNative collation);

        IntPtr MarshalDelegateToNativeFunctionPointer(Delegate del);

        void Copy(IntPtr source, byte[] destination, int startIndex, int length);

        void Copy(byte[] source, IntPtr destination, int startIndex, int length);
    }
}