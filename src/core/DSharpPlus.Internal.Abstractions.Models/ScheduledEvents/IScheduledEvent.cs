// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a guild scheduled event.
/// </summary>
public interface IScheduledEvent : IPartialScheduledEvent
{
    /// <inheritdoc cref="IPartialScheduledEvent.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialScheduledEvent.GuildId"/>
    public new Snowflake GuildId { get; }

    /// <inheritdoc cref="IPartialScheduledEvent.ChannelId"/>
    public new Snowflake ChannelId { get; }

    /// <inheritdoc cref="IPartialScheduledEvent.Name"/>
    public new string Name { get; }

    /// <inheritdoc cref="IPartialScheduledEvent.ScheduledStartTime"/>
    public new DateTimeOffset ScheduledStartTime { get; }

    /// <inheritdoc cref="IPartialScheduledEvent.PrivacyLevel"/>
    public new DiscordScheduledEventPrivacyLevel PrivacyLevel { get; }

    /// <inheritdoc cref="IPartialScheduledEvent.Status"/>
    public new DiscordScheduledEventStatus Status { get; }

    /// <inheritdoc cref="IPartialScheduledEvent.EntityType"/>
    public new DiscordScheduledEventType EntityType { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<Snowflake> IPartialScheduledEvent.Id => this.Id;

    /// <inheritdoc/>
    Optional<Snowflake> IPartialScheduledEvent.GuildId => this.GuildId;

    /// <inheritdoc/>
    Optional<Snowflake> IPartialScheduledEvent.ChannelId => this.ChannelId;

    /// <inheritdoc/>
    Optional<string> IPartialScheduledEvent.Name => this.Name;

    /// <inheritdoc/>
    Optional<DateTimeOffset> IPartialScheduledEvent.ScheduledStartTime => this.ScheduledStartTime;

    /// <inheritdoc/>
    Optional<DiscordScheduledEventPrivacyLevel> IPartialScheduledEvent.PrivacyLevel => this.PrivacyLevel;

    /// <inheritdoc/>
    Optional<DiscordScheduledEventStatus> IPartialScheduledEvent.Status => this.Status;

    /// <inheritdoc/>
    Optional<DiscordScheduledEventType> IPartialScheduledEvent.EntityType => this.EntityType;
}
