// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Results.Errors;

namespace DSharpPlus.Internal.Abstractions.Rest.Errors;

/// <summary>
/// Represents an error encountered during parameter validation.
/// </summary>
/// <param name="message">The human-readable error message.</param>
public record ValidationError(string message) : Error(message);
