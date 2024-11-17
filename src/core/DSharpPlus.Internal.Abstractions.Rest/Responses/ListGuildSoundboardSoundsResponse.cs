// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Responses;

/// <summary>
/// Represents a response from <c>GET guilds/:guild-id/soundboard-sounds</c>.
/// </summary>
public readonly record struct ListGuildSoundboardSoundsResponse
{
    /// <summary>
    /// The soundboard sounds present in the provided guild.
    /// </summary>
    public IReadOnlyList<ISoundboardSound> Items { get; init; }
}
