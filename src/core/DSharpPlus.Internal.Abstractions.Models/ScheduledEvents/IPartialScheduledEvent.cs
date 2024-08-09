// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a partially populated scheduled event.
/// </summary>

// we deliberately ignore some fields listed in the documentation, because the documentation is outdated.
// it still documents data for external events (status: 2023-07-01) despite having been removed a long
// time ago...
// the following changes are thereby made from the documentation:
// - remove entity_metadata
// - remove scheduled_end_time
// - change channel_id to non-nullable
public interface IPartialScheduledEvent
{
    /// <summary>
    /// The snowflake identifier of the scheduled event.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The snowflake identifier of the guild this event belongs to.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// The snowflake identifier of the channel in which this event will be hosted.
    /// </summary>
    public Optional<Snowflake> ChannelId { get; }

    /// <summary>
    /// The snowflake identifier of the user that created this event.
    /// </summary>
    public Optional<Snowflake> CreatorId { get; }

    /// <summary>
    /// The name of this event, 1 to 100 characters.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The description of this event, 1 to 1000 characters.
    /// </summary>
    public Optional<string?> Description { get; }

    /// <summary>
    /// The time at which this scheduled event will start.
    /// </summary>
    public Optional<DateTimeOffset> ScheduledStartTime { get; }

    /// <summary>
    /// The privacy level of this event.
    /// </summary>
    public Optional<DiscordScheduledEventPrivacyLevel> PrivacyLevel { get; }

    /// <summary>
    /// The status of this scheduled event.
    /// </summary>
    public Optional<DiscordScheduledEventStatus> Status { get; }

    /// <summary>
    /// The type of this scheduled event.
    /// </summary>
    public Optional<DiscordScheduledEventType> EntityType { get; }

    /// <summary>
    /// The user that created this event.
    /// </summary>
    public Optional<IUser> Creator { get; }

    /// <summary>
    /// The number of users subscribed to this event.
    /// </summary>
    public Optional<int> UserCount { get; }

    /// <summary>
    /// The cover image hash of this event.
    /// </summary>
    public Optional<string?> Image { get; }

    /// <summary>
    /// A definition for how often and at what dates this event should recur.
    /// </summary>
    public Optional<IScheduledEventRecurrenceRule?> RecurrenceRule { get; }
}
