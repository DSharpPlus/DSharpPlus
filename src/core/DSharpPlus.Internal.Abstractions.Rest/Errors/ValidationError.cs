// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using Remora.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.Errors;

/// <summary>
/// Represents an error encountered during parameter validation.
/// </summary>
public record ValidationError : ResultError
{
    /// <summary>
    /// If this error was caused by any specific parameters, contains the names of the invalid parameters.
    /// </summary>
    public IReadOnlyList<string>? ParameterNames { get; init; }

    /// <summary>
    /// Initializes a new validation error.
    /// </summary>
    /// <param name="message">The human-readable error message.</param>
    /// <param name="parameters">If applicable, the names of parameters that failed validation.</param>
    public ValidationError(string message, params string[] parameters)
        : base(message) 
        => this.ParameterNames = parameters.Length != 0 ? parameters : null;
}
