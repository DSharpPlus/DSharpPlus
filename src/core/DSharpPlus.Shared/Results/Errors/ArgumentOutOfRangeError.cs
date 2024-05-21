// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Results.Errors;

/// <summary>
/// An error returned when a provided argument was null.
/// </summary>
public record ArgumentOutOfRangeError : ArgumentError
{
    /// <summary>
    /// Creates a new <see cref="ArgumentOutOfRangeError"/> with the specified message and an unspecified argument name.
    /// </summary>
    public ArgumentOutOfRangeError(string message) : base(message)
    {

    }

    /// <summary>
    /// Creates a new <see cref="ArgumentOutOfRangeError"/> with the specified message and argument name.
    /// </summary>
    public ArgumentOutOfRangeError(string message, string argumentName) : base(message, argumentName)
    {

    }

    /// <summary>
    /// Creates a new <see cref="ArgumentOutOfRangeError"/> from the specified exception.
    /// </summary>
    public ArgumentOutOfRangeError(Exception exception) : base(exception)
    {
        this.Message = exception.Message;
        this.ArgumentName = exception is ArgumentOutOfRangeException { ParamName: { } argument } ? argument : "Unspecified.";
    }

    public override Exception ToException() => new ArgumentOutOfRangeException(this.Message, this.ArgumentName);
}
