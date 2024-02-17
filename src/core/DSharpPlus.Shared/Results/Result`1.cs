// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

using DSharpPlus.Results.Errors;

namespace DSharpPlus.Results;

/// <summary>
/// Represents the success or failure of an operation, and the error or value returned as applicable.
/// </summary>
public readonly record struct Result<TValue>
{
    /// <summary>
    /// The value this operation returned, if applicable.
    /// </summary>
    [AllowNull]
    public TValue Value { get; init; }

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
    public Result(Error error)
        => this.Error = error;

    /// <summary>
    /// Constructs a new successful result from the specified value.
    /// </summary>
    public Result(TValue value)
        => this.Value = value;

    public static implicit operator Result<TValue>(Error error)
        => new(error);

    public static implicit operator Result<TValue>(TValue value)
        => new(value);
}
