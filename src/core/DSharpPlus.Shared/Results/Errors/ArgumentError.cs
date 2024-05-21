// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Results.Errors;

/// <summary>
/// An error returned when a provided argument was invalid.
/// </summary>
public record ArgumentError : ExceptionError
{
    /// <summary>
    /// The name of the invalid argument.
    /// </summary>
    public string ArgumentName { get; private protected set; }

    /// <summary>
    /// Creates a new <see cref="ArgumentError"/> with the specified message and an unspecified argument name.
    /// </summary>
    public ArgumentError(string message) : base(message)
        => this.ArgumentName = "Unspecified.";

    /// <summary>
    /// Creates a new <see cref="ArgumentError"/> with the specified message and argument name.
    /// </summary>
    public ArgumentError(string message, string argumentName) : base(message)
        => this.ArgumentName = argumentName;

    /// <summary>
    /// Creates a new <see cref="ArgumentError"/> from the specified exception.
    /// </summary>
    public ArgumentError(Exception exception) : base(exception)
    {
        this.Message = exception.Message;
        this.ArgumentName = exception is ArgumentException { ParamName: { } argument } ? argument : "Unspecified.";
    }

    public override Exception ToException() => new ArgumentException(this.Message, this.ArgumentName);
}
