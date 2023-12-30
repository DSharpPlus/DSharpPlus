// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.Errors;

/// <summary>
/// Represents an error encountered during parameter validation.
/// </summary>
public sealed record ValidationError : ResultError
{
    /// <summary>
    /// Initializes a new validation error.
    /// </summary>
    /// <param name="message">The human-readable error message.</param>
    public ValidationError(string message)
        : base(message)
    {

    }
}
