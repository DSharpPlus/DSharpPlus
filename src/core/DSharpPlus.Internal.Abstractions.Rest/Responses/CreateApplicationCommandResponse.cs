// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Responses;

/// <summary>
/// Represents the information returned by <c>POST /applications/:application-id/commands</c>.
/// </summary>
public readonly record struct CreateApplicationCommandResponse
{
    /// <summary>
    /// Indicates whether this command was newly created or whether it already existed.
    /// </summary>
    public required bool IsNewlyCreated { get; init;  }

    /// <summary>
    /// The created command.
    /// </summary>
    public required IApplicationCommand CreatedCommand { get; init; }
}
