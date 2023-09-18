// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DSharpPlus.Etf.Serialization;

/// <summary>
/// Provides methods to relegate <see langword="throw"/>s to, to work around inlining limitations in the JIT.
/// </summary>
internal static class ThrowHelper
{
    [DoesNotReturn]
    [DebuggerHidden]
    [StackTraceHidden]
    public static void ThrowInvalidDecode
    (
        Type target
    )
        => throw new InvalidOperationException($"Failed to decode the current term into an object of type {target}.");
}
