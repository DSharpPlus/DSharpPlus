// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

using DSharpPlus.Results.Errors;

namespace DSharpPlus.Results;

/// <summary>
/// Represents the success or failure of an operation, and optionally the error returned.
/// </summary>
public readonly record struct Result
{
    /// <summary>
    /// The error this operation returned, if applicable.
    /// </summary>
    public Error? Error { get; init; }

    /// <summary>
    /// Indicates whether this operation was successful.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => this.Error is null;

    /// <summary>
    /// Constructs a new result from the specified failure case.
    /// </summary>
    /// <param name="error"></param>
    public Result(Error error)
        => this.Error = error;

    public static implicit operator Result(Error error)
        => new(error);
}
