// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#pragma warning disable CA2201

using System;

namespace DSharpPlus.Results.Errors;

/// <summary>
/// Provides a base class for errors that can map to an exception.
/// </summary>
public abstract record ExceptionError : Error
{
    /// <summary>
    /// Converts this error into a throwable exception.
    /// </summary>
    public virtual Exception ToException() => new(this.Message);

    protected ExceptionError(string message) : base(message)
    {

    }

    /// <summary>
    /// Override this constructor to provide a way to be constructible from an exception.
    /// </summary>
    protected ExceptionError(Exception exception) : base(exception.Message)
    {

    }
}
