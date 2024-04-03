// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using DSharpPlus.Results.Errors;
using DSharpPlus.Results.ExceptionServices;

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

    public static implicit operator bool(Result result)
        => result.IsSuccess;

    /// <summary>
    /// Throws the result error as an exception, if applicable.
    /// </summary>
    [DebuggerHidden]
    [StackTraceHidden]
    public void Expect()
    {
        if (!this.IsSuccess)
        {
            throw ExceptionMarshaller.MarshalResultErrorToException(this.Error);
        }
    }

    /// <summary>
    /// Throws the result error as an exception according to the provided transform, if applicable.
    /// </summary>
    [DebuggerHidden]
    [StackTraceHidden]
    public void Expect(Func<Result, Exception> transform)
    {
        if (!this.IsSuccess)
        {
            throw transform(this);
        }
    }

    /// <summary>
    /// Transforms the result error according to the provided function, returning either a successful result or the transformed result.
    /// </summary>
    public Result MapError(Func<Error, Error> transform)
        => this.IsSuccess ? this : new(transform(this.Error));
}
