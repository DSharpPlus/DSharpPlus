// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
#if !NETSTANDARD
using System.Diagnostics.CodeAnalysis;
#endif

namespace DSharpPlus.Collections;

/// <summary>
/// Contains utilities for throwing exceptions out of hot code paths.
/// </summary>
internal static class ThrowHelper
{
    [DebuggerHidden]
#if !NETSTANDARD
    [DoesNotReturn]
    [StackTraceHidden]
#endif
    internal static void ThrowConcurrentOperationsNotSupported() 
        => throw new InvalidOperationException("Concurrent modifications to this collection type are not supported.");

    [DebuggerHidden]
#if !NETSTANDARD
    [DoesNotReturn]
    [StackTraceHidden]
#endif
    internal static void ThrowCapacityIntMaxValueExceeded()
        => throw new InvalidOperationException($"This type's maximum capacity of {int.MaxValue} was exceeded.");

    [DebuggerHidden]
#if !NETSTANDARD
    [DoesNotReturn]
    [StackTraceHidden]
#endif
    internal static void ThrowValueNotFound()
        => throw new ArgumentException("There was no value corresponding to the given key.");
}
