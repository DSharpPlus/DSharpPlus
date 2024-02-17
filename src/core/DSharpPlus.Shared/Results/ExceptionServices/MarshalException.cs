// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA1032

using System;
using System.Diagnostics.CodeAnalysis;

using DSharpPlus.Results.Errors;

namespace DSharpPlus.Results.ExceptionServices;

public sealed class MarshalException : Exception
{
    /// <summary>
    /// Gets the result error this was originally 
    /// </summary>
    public required Error Error { get; init; }

    [SetsRequiredMembers]
    public MarshalException(Error underlying) : base("Failed to find a matching exception type for this result error.")
        => this.Error = underlying;
}
