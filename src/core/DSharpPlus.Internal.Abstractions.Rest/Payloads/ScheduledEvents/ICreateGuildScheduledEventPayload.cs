// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /guilds/:guild-id/scheduled-events</c>.
/// </summary>
public interface ICreateGuildScheduledEventPayload
{
    /// <summary>
    /// The channel ID the scheduled event will take place in.
    /// </summary>
    public Optional<Snowflake> ChannelId { get; }

    /// <summary>
    /// Represents metadata about the scheduled event.
    /// </summary>
    public Optional<IScheduledEventMetadata> EntityMetadata { get; }

    /// <summary>
    /// Name of the scheduled event.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Privacy level for this scheduled event.
    /// </summary>
    public DiscordScheduledEventPrivacyLevel PrivacyLevel { get; }

    /// <summary>
    /// Indicates the time at which this event is scheduled to start.
    /// </summary>
    public DateTimeOffset ScheduledStartTime { get; }

    /// <summary>
    /// Indicates the time at which this event is scheduled to end.
    /// </summary>
    public Optional<DateTimeOffset> ScheduledEndTime { get; }

    /// <summary>
    /// Description for this scheduled event.
    /// </summary>
    public Optional<string> Description { get; }

    /// <summary>
    /// The event type of this event.
    /// </summary>
    public DiscordScheduledEventType EntityType { get; }

    /// <summary>
    /// Image data representing the cover image of this scheduled event.
    /// </summary>
    public Optional<InlineMediaData> Image { get; }

    /// <summary>
    /// A definition for how often and at what dates this event should recur.
    /// </summary>
    public Optional<IScheduledEventRecurrenceRule> RecurrenceRule { get; }
}
