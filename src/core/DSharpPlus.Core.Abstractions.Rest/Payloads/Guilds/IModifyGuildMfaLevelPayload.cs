// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /guilds/:guild-id/mfa</c>.
/// </summary>
public interface IModifyGuildMfaLevelPayload
{
    /// <summary>
    /// The new MFA level for this guild.
    /// </summary>
    public DiscordMfaLevel Level { get; }
}
