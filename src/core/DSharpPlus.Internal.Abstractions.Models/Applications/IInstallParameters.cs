// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Stores metadata about the installation process for an application.
/// </summary>
public interface IInstallParameters
{
    /// <summary>
    /// The OAuth2 scopes to add the application to the server with.
    /// </summary>
    public IReadOnlyList<string> Scopes { get; }

    /// <summary>
    /// The permissions to request for the bot role.
    /// </summary>
    public DiscordPermissions Permissions { get; }
}
