// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DSharpPlus;

/// <summary>
/// Contains methods relegating to throw statements.
/// </summary>
internal static class ThrowHelper
{
    [DoesNotReturn]
    [DebuggerHidden]
    [StackTraceHidden]
    public static void ThrowOptionalNoValuePresent()
        => throw new InvalidOperationException("This optional did not have a value specified.");

    [DoesNotReturn]
    [DebuggerHidden]
    [StackTraceHidden]
    public static void ThrowFunc
    (
        Func<Exception> func
    )
        => throw func();
}
