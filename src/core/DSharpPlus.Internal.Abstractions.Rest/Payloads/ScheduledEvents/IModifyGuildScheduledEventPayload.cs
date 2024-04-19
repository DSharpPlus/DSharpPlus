// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /guilds/:guild-id/scheduled-events/:event-id</c>.
/// </summary>
public interface IModifyGuildScheduledEventPayload
{
    /// <summary>
    /// The channel ID the scheduled event will take place in.
    /// </summary>
    public Optional<Snowflake?> ChannelId { get; }

    /// <summary>
    /// Represents metadata about the scheduled event.
    /// </summary>
    public Optional<IScheduledEventMetadata?> EntityMetadata { get; }

    /// <summary>
    /// Name of the scheduled event.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// Privacy level for this scheduled event.
    /// </summary>
    public Optional<DiscordScheduledEventPrivacyLevel> PrivacyLevel { get; }

    /// <summary>
    /// Indicates the time at which this event is scheduled to start.
    /// </summary>
    public Optional<DateTimeOffset> ScheduledStartTime { get; }

    /// <summary>
    /// Indicates the time at which this event is scheduled to end.
    /// </summary>
    public Optional<DateTimeOffset> ScheduledEndTime { get; }

    /// <summary>
    /// Description for this scheduled event.
    /// </summary>
    public Optional<string?> Description { get; }

    /// <summary>
    /// The event type of this event.
    /// </summary>
    public Optional<DiscordScheduledEventType> EntityType { get; }

    /// <summary>
    /// The status of this scheduled event. To start or end an event, set this field to its respective state.
    /// </summary>
    public Optional<DiscordScheduledEventStatus> Status { get; }

    /// <summary>
    /// Image data representing the cover image of this scheduled event.
    /// </summary>
    public Optional<ImageData> Image { get; }
}
