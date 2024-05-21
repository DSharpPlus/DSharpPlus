// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Results.Errors;

/// <summary>
/// An error returned when a provided argument was null.
/// </summary>
public record ArgumentNullError : ArgumentError
{
    /// <summary>
    /// Creates a new <see cref="ArgumentNullError"/> with the default message and specified argument name.
    /// </summary>
    public ArgumentNullError(string argumentName) : base("The provided value must not be null", argumentName)
    {

    }

    /// <summary>
    /// Creates a new <see cref="ArgumentNullError"/> with the specified message and argument name.
    /// </summary>
    public ArgumentNullError(string message, string argumentName) : base(message, argumentName)
    {

    }

    /// <summary>
    /// Creates a new <see cref="ArgumentNullError"/> from the specified exception.
    /// </summary>
    public ArgumentNullError(Exception exception) : base(exception)
    {
        this.Message = exception.Message;
        this.ArgumentName = exception is ArgumentNullException { ParamName: { } argument } ? argument : "Unspecified.";
    }

    public override Exception ToException() => new ArgumentNullException(this.Message, this.ArgumentName);
}
