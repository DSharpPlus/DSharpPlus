// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a user's voice connection status.
/// </summary>
public interface IVoiceState
{
    /// <summary>
    /// The snowflake identifier of the guild this voice state is for.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// The snowflake identifier of the channel this user is connected to, <see langword="null"/> if none.
    /// </summary>
    public Snowflake? ChannelId { get; }

    /// <summary>
    /// The snowflake identifier of the user this voice state is for.
    /// </summary>
    public Snowflake UserId { get; }

    /// <summary>
    /// The guild member this voice state is for.
    /// </summary>
    public Optional<IGuildMember> Member { get; }

    /// <summary>
    /// The session identifier for this voice state.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Indicates whether this user is deafened by the server.
    /// </summary>
    public bool Deaf { get; }

    /// <summary>
    /// Indicates whether this user is muted by the server.
    /// </summary>
    public bool Mute { get; }

    /// <summary>
    /// Indicates whether this user has deafened themselves.
    /// </summary>
    public bool SelfDeaf { get; }

    /// <summary>
    /// Indicates whether this user has muted themselves.
    /// </summary>
    public bool SelfMute { get; }

    /// <summary>
    /// Indicates whether this user is streaming in the voice channel.
    /// </summary>
    public Optional<bool> SelfStream { get; }

    /// <summary>
    /// Indicates whether this user's camera is enabled.
    /// </summary>
    public bool SelfVideo { get; }

    /// <summary>
    /// Indicates whether this user's permission to speak is denied.
    /// </summary>
    public bool Suppress { get; }

    /// <summary>
    /// The time at which this user requested to speak.
    /// </summary>
    public DateTimeOffset? RequestToSpeakTimestamp { get; }
}
