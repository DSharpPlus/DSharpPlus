// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Results.Errors;

/// <summary>
/// Represents an error that has occurred during the operation described by the Result containing this error.
/// </summary>
public abstract record Error
{
    /// <summary>
    /// The human-readable error message.
    /// </summary>
    public string Message { get; init; }

    protected Error(string message)
        => this.Message = message;
}
