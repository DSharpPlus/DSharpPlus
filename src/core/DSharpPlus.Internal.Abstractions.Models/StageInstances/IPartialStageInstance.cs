// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a partial stage instance.
/// </summary>
public interface IPartialStageInstance
{
    /// <summary>
    /// The snowflake identifier of this stage instance.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The snowflake identifier of the guild this stage instance is held in.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// The snowflake identifier of the associated stage channel.
    /// </summary>
    public Optional<Snowflake> ChannelId { get; }

    /// <summary>
    /// The topic of this stage instance, 1 to 120 characters.
    /// </summary>
    public Optional<string> Topic { get; }

    /// <summary>
    /// The privacy level of this stage instance.
    /// </summary>
    public Optional<DiscordStagePrivacyLevel> PrivacyLevel { get; }

    /// <summary>
    /// The snowflake identifier of the scheduled event for this stage instance.
    /// </summary>
    public Optional<Snowflake?> GuildScheduledEventId { get; }
}
