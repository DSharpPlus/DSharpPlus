// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /guilds/:guild-id/welcome-screen</c>.
/// </summary>
public interface IModifyGuildWelcomeScreenPayload
{
    /// <summary>
    /// Indicates whether the welcome screen is enabled.
    /// </summary>
    public Optional<bool?> Enabled { get; }

    /// <summary>
    /// The channels linked in the welcome screen with their display options.
    /// </summary>
    public Optional<IReadOnlyList<IWelcomeScreenChannel>?> WelcomeChannels { get; }

    /// <summary>
    /// The guild description to show in the welcome screen.
    /// </summary>
    public Optional<string?> Description { get; }
}
