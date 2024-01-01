// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Remora.Results;

namespace DSharpPlus.Extensions.Internal.Builders.Errors;

/// <summary>
/// Represents an error encountered when attempting to validate a builder.
/// </summary>
public record BuilderValidationError : ResultError
{
    /// <summary>
    /// If this error was caused by any specific parameters, contains the names of the invalid parameters.
    /// </summary>
    public IReadOnlyDictionary<string, string>? ParameterNames { get; init; }

    /// <summary>
    /// Initializes a new validation error.
    /// </summary>
    /// <param name="message">The human-readable error message.</param>
    /// <param name="parameters">If applicable, the names of parameters that failed validation.</param>
    public BuilderValidationError(string message, params (string Key, string Value)[] parameters)
        : base(message) 
    {
        if (parameters.Length == 0)
        {
            return;
        }

        this.ParameterNames = parameters.ToDictionary(x => x.Key, x => x.Value);
    }

    public override string ToString()
    {
        if (this.ParameterNames is null)
        {
            return base.ToString();
        }

        StringBuilder builder = new($"BuilderValidationError\n{{\n\t{this.Message},\n\tParameters:\n\t[");

        foreach (KeyValuePair<string, string> kvp in this.ParameterNames)
        {
            builder.Append(CultureInfo.InvariantCulture, $"\n\t\t{kvp.Key}: {kvp.Value}");
        }

        builder.Append("\n\t]\n}");

        return builder.ToString();
    }
}