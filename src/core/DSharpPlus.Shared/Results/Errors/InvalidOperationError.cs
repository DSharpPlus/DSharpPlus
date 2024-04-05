// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

namespace DSharpPlus.Results.Errors;

/// <summary>
/// An error indicating an invalid operation.
/// </summary>
public record InvalidOperationError : ExceptionError
{
    /// <summary>
    /// Creates a new <seealso cref="InvalidOperationError"/> with the specified message.
    /// </summary>
    public InvalidOperationError(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new <seealso cref="InvalidOperationError"/> from the specified exception.
    /// </summary>
    public InvalidOperationError(Exception exception) : base(exception)
        => this.Message = exception.Message;

    public override Exception ToException() => new InvalidOperationException(this.Message);
}
