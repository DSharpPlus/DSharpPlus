// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a live stage channel.
/// </summary>
public interface IStageInstance : IPartialStageInstance
{
    /// <inheritdoc cref="IPartialStageInstance.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialStageInstance.GuildId"/>
    public new Snowflake GuildId { get; }

    /// <inheritdoc cref="IPartialStageInstance.ChannelId"/>
    public new Snowflake ChannelId { get; }

    /// <inheritdoc cref="IPartialStageInstance.Topic"/>
    public new string Topic { get; }

    /// <inheritdoc cref="IPartialStageInstance.PrivacyLevel"/>
    public new DiscordStagePrivacyLevel PrivacyLevel { get; }

    /// <inheritdoc cref="IPartialStageInstance.GuildScheduledEventId"/>
    public new Snowflake? GuildScheduledEventId { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<Snowflake> IPartialStageInstance.Id => this.Id;

    /// <inheritdoc/>
    Optional<Snowflake> IPartialStageInstance.GuildId => this.GuildId;

    /// <inheritdoc/>
    Optional<Snowflake> IPartialStageInstance.ChannelId => this.ChannelId;

    /// <inheritdoc/>
    Optional<string> IPartialStageInstance.Topic => this.Topic;

    /// <inheritdoc/>
    Optional<DiscordStagePrivacyLevel> IPartialStageInstance.PrivacyLevel => this.PrivacyLevel;

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialStageInstance.GuildScheduledEventId => this.GuildScheduledEventId;
}
