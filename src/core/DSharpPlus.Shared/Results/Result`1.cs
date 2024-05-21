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
/// Represents the success or failure of an operation, and the error or value returned as applicable.
/// </summary>
public readonly record struct Result<TValue>
{
    /// <summary>
    /// The value this operation returned, if applicable.
    /// </summary>
    [AllowNull]
    public TValue Value { get; private init; }

    /// <summary>
    /// The error this operation returned, if applicable.
    /// </summary>
    public Error? Error { get; private init; }

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

    public static implicit operator bool(Result<TValue> result)
        => result.IsSuccess;

    public static implicit operator Result(Result<TValue> result)
        => result.IsSuccess ? Result.Success : new(result.Error);

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
    public void Expect(Func<Error, Exception> transform)
    {
        if (!this.IsSuccess)
        {
            throw transform(this.Error);
        }
    }

    /// <summary>
    /// Unwraps the result, throwing an exception if unsuccessful.
    /// </summary>
    public TValue Unwrap()
    {
        this.Expect();
        return this.Value;
    }

    /// <summary>
    /// Unwraps the result, returning a provided default value if unsuccessful.
    /// </summary>
    public TValue UnwrapOr(TValue defaultValue)
        => this.IsSuccess ? this.Value : defaultValue;

    /// <summary>
    /// Unwraps the result, returning the default value if unsuccessful.
    /// </summary>
    public TValue? UnwrapOrDefault()
        => this.IsSuccess ? this.Value : default;

    /// <summary>
    /// Unwraps the result, returning the result of evaluating the provided function if unsuccessful.
    /// </summary>
    public TValue UnwrapOrElse(Func<Result<TValue>, TValue> fallback)
        => this.IsSuccess ? this.Value : fallback(this);

    /// <summary>
    /// Transforms the result value according to the provided function, returning either the transformed result or a failed result.
    /// </summary>
    public Result<TResult> Map<TResult>(Func<TValue, TResult> transform)
        => this.IsSuccess ? new(transform(this.Value)) : new(this.Error);

    /// <summary>
    /// Transforms the result error according to the provided function, returning either a successful result or the transformed result.
    /// </summary>
    public Result<TValue> MapError(Func<Error, Error> transform)
        => this.IsSuccess ? this : new(transform(this.Error));

    /// <summary>
    /// Transforms the result value according to the provided function, returning either the transformed result or the provided default value.
    /// </summary>
    public Result<TResult?> MapOr<TResult>(Func<TValue, TResult> transform, TResult fallback)
        => this.IsSuccess ? new(transform(this.Value)) : new(fallback);

    /// <summary>
    /// Transforms the result value according to the provided function, returning either the transformed result or a default value.
    /// </summary>
    public Result<TResult?> MapOrDefault<TResult>(Func<TValue, TResult> transform)
        => this.IsSuccess ? new(transform(this.Value)) : new(default(TResult));

    /// <summary>
    /// Transforms the result value and error according to the provided functions.
    /// </summary>
    public Result<TResult> MapOrElse<TResult>(Func<TValue, TResult> transformValue, Func<Error, Error> transformError)
        => this.IsSuccess ? new(transformValue(this.Value)) : new(transformError(this.Error));

#pragma warning disable CA1000

    /// <summary>
    /// Creates a new failed result from the specified error.
    /// </summary>
    public static Result<TValue> FromError(Error error) => new(error);

    /// <summary>
    /// Creates a new successful result from the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<TValue> FromSuccess(TValue value) => new(value);

#pragma warning restore CA1000
}
