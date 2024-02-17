// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;

using DSharpPlus.Results.Errors;

namespace DSharpPlus.Results.ExceptionServices;

/// <summary>
/// Used by the result-exception marshaller when failing to find a matching result error.
/// </summary>
public sealed record MarshalError : Error
{
    /// <summary>
    /// Gets the exception that failed to marshal.
    /// </summary>
    public required Exception Exception { get; init; }

    [SetsRequiredMembers]
    public MarshalError(Exception exception) : base("Failed to find a suitable result type for the exception.")
        => this.Exception = exception;
}
