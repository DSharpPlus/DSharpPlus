// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /guilds/:guild-id/voice-states/:user-id</c>.
/// </summary>
public interface IModifyUserVoiceStatePayload
{
    /// <summary>
    /// The snowflake identifier of the channel this user is currently in.
    /// </summary>
    public Snowflake ChannelId { get; }

    /// <summary>
    /// Toggles this user's suppression state.
    /// </summary>
    public Optional<bool> Suppress { get; }
}
