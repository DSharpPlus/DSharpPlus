// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable IDE0046

using System;

using DSharpPlus.Results.Errors;

namespace DSharpPlus.Results.ExceptionServices;

/// <summary>
/// Provides a way to marshal results to exceptions, if erroneous. Exceptions and result may not be able to be round-tripped.
/// </summary>
public static class ExceptionMarshaller
{
    /// <summary>
    /// Creates an exception from the specified result error.
    /// </summary>
    public static Exception MarshalResultErrorToException(Error error)
    {
        // if we previously failed to marshal this into an error, just get the original exception
        if (error is MarshalError marshalError)
        {
            return marshalError.Exception;
        }

        // if this is freely convertible, just do that
        if (error is ExceptionError exceptionError)
        {
            return exceptionError.ToException();
        }

        return new MarshalException(error);
    }

    /// <summary>
    /// Creates a result error from the specified exception.
    /// </summary>
    public static Error MarshalExceptionToResultError(Exception exception)
    {
        if (exception is MarshalException marshalException)
        {
            return marshalException.Error;
        }

        // see if we can find a matching error in a sufficiently similarly named namespace,
        // ie DSharpPlus.Exceptions.DiscordException -> DSharpPlus.Errors.DiscordError
        if (Type.GetType(exception.GetType().FullName!.Replace("Exception", "Error", StringComparison.Ordinal)) is Type candidate)
        {
            if (candidate.IsAssignableTo(typeof(ExceptionError)))
            {
                return (Error)candidate.GetConstructor([typeof(Exception)])!.Invoke([exception]);
            }
        }

        // try replacing the namespace with DSharpPlus.Results.Errors, too
        if
        (
            Type.GetType
            (
                $"DSharpPlus.Results.Errors.{exception.GetType().Name!.Replace("Exception", "Error", StringComparison.Ordinal)}"
            ) is Type secondCandidate
        )
        {
            if (secondCandidate.IsAssignableTo(typeof(ExceptionError)))
            {
                return (Error)secondCandidate.GetConstructor([typeof(Exception)])!.Invoke([exception]);
            }
        }

        return new MarshalError(exception);
    }

    /// <summary>
    /// Creates an exception from the specified result, if erroneous.
    /// </summary>
    public static Exception? MarshalResultToException(Result result)
    {
        if (result.IsSuccess)
        {
            return null;
        }

        return MarshalResultErrorToException(result.Error);
    }

    /// <summary>
    /// Creates an exception from the specified generic result, if erroneous.
    /// </summary>
    public static Exception? MarshalResultToException<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return null;
        }

        return MarshalResultErrorToException(result.Error);
    }

    /// <summary>
    /// Creates an erroneous result from the specified exception.
    /// </summary>
    public static Result MarshalExceptionToResult(Exception exception)
        => new(MarshalExceptionToResultError(exception));

    /// <summary>
    /// Creates an erroneous generic result from the specified exception.
    /// </summary>
    public static Result<T> MarshalExceptionToResult<T>(Exception exception)
        => new(MarshalExceptionToResultError(exception));
}
