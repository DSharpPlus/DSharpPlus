// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using DSharpPlus.Etf.Extensions;

namespace DSharpPlus.Etf.Serialization;

partial struct EtfReader
{
    /// <summary>
    /// Reads the current term as a string.
    /// </summary>
    /// <returns>True if reading was successful, false if unsuccessful.</returns>
    public readonly bool TryReadString
    (
        [NotNullWhen(true)]
        out string? value
    )
    {
        if (this.TermType.IsString())
        {
            value = Encoding.UTF8.GetString(this.CurrentTermContents);
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Copies the current term to a Span as UTF-8 string.
    /// </summary>
    /// <returns>True if successful, false if unsuccessful.</returns>
    public readonly bool TryReadUtf8String
    (
        Span<byte> buffer
    ) 
        => this.TermType.IsString() && this.CurrentTermContents.TryCopyTo(buffer);

    /// <summary>
    /// Reads the current term as a string.
    /// </summary>
    public readonly string ReadString()
    {
        if (this.TryReadString(out string? value))
        {
            return value;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(string));
        return default!;
    }

    /// <summary>
    /// Copies the current term to a Span as UTF-8 string.
    /// </summary>
    public readonly void ReadUtf8String
    (
        Span<byte> buffer
    )
    {
        if (this.TryReadUtf8String(buffer))
        {
            return;
        }

        ThrowHelper.ThrowInvalidDecode(typeof(string));
    }
}
