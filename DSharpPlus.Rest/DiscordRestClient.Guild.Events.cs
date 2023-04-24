// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;

namespace DSharpPlus;

public sealed partial class DiscordRestClient
{
    /// <summary>
    /// Creates a new scheduled guild event.
    /// </summary>
    /// <param name="guildId">The guild to create an event on.</param>
    /// <param name="name">The name of the event, up to 100 characters.</param>
    /// <param name="description">The description of the event, up to 1000 characters.</param>
    /// <param name="channelId">The channel the event will take place in, if applicable.</param>
    /// <param name="type">The type of event. If <see cref="ScheduledGuildEventType.External"/>, a end time must be specified.</param>
    /// <param name="privacyLevel">The privacy level of the event.</param>
    /// <param name="start">When the event starts. Must be in the future and before the end date, if specified.</param>
    /// <param name="end">When the event ends. Required for <see cref="ScheduledGuildEventType.External"/></param>
    /// <param name="location">Where this location takes place.</param>
    /// <returns>The created event.</returns>
    public Task<DiscordScheduledGuildEvent> CreateScheduledGuildEventAsync
    (
        ulong guildId,
        string name,
        string description,
        ulong? channelId,
        ScheduledGuildEventType type,
        ScheduledGuildEventPrivacyLevel privacyLevel,
        DateTimeOffset start,
        DateTimeOffset? end,
        string? location = null
    ) => ApiClient.CreateScheduledGuildEventAsync
    (
        guildId,
        name,
        description,
        channelId,
        start,
        end,
        type,
        privacyLevel,
        new DiscordScheduledGuildEventMetadata(location)
    );

    /// <summary>
    /// Delete a scheduled guild event.
    /// </summary>
    /// <param name="guildId">The ID the guild the event resides on.</param>
    /// <param name="eventId">The ID of the event to delete.</param>
    public Task DeleteScheduledGuildEventAsync(ulong guildId, ulong eventId)
        => ApiClient.DeleteScheduledGuildEventAsync(guildId, eventId);

    /// <summary>
    /// Gets a specific scheduled guild event.
    /// </summary>
    /// <param name="guildId">The ID of the guild the event resides on.</param>
    /// <param name="eventId">The ID of the event to get</param>
    /// <returns>The requested event.</returns>
    public Task<DiscordScheduledGuildEvent> GetScheduledGuildEventAsync(ulong guildId, ulong eventId)
        => ApiClient.GetScheduledGuildEventAsync(guildId, eventId);

    /// <summary>
    /// Gets all available scheduled guild events.
    /// </summary>
    /// <param name="guildId">The ID of the guild to query.</param>
    /// <returns>All active and scheduled events.</returns>
    public Task<IReadOnlyList<DiscordScheduledGuildEvent>> GetScheduledGuildEventsAsync(ulong guildId)
        => ApiClient.GetScheduledGuildEventsAsync(guildId);

    /// <summary>
    /// Modify a scheduled guild event.
    /// </summary>
    /// <param name="guildId">The ID of the guild the event resides on.</param>
    /// <param name="eventId">The ID of the event to modify.</param>
    /// <param name="action">The action to apply to the event.</param>
    /// <returns>The modified event.</returns>
    public Task<DiscordScheduledGuildEvent> ModifyScheduledGuildEventAsync(ulong guildId, ulong eventId, Action<ScheduledGuildEventEditModel> action)
    {
        ScheduledGuildEventEditModel scheduledGuildEventEditModel = new ScheduledGuildEventEditModel();
        action(scheduledGuildEventEditModel);

        if (scheduledGuildEventEditModel.Type.HasValue)
        {
            switch (scheduledGuildEventEditModel.Type.Value)
            {
                case ScheduledGuildEventType.StageInstance:
                case ScheduledGuildEventType.VoiceChannel:
                    if (!scheduledGuildEventEditModel.Channel.HasValue)
                    {
                        throw new ArgumentException("Channel must be supplied if the event is a stage instance or voice channel event.");
                    }

                    break;
                case ScheduledGuildEventType.External:
                    if (!scheduledGuildEventEditModel.EndTime.HasValue)
                    {
                        throw new ArgumentException("End must be supplied if the event is an external event.");
                    }
                    else if (!scheduledGuildEventEditModel.Metadata.HasValue || string.IsNullOrEmpty(scheduledGuildEventEditModel.Metadata.Value.Location))
                    {
                        throw new ArgumentException("Location must be supplied if the event is an external event.");
                    }
                    else if (scheduledGuildEventEditModel.Channel.HasValue && scheduledGuildEventEditModel.Channel.Value != null)
                    {
                        throw new ArgumentException("Channel must not be supplied if the event is an external event.");
                    }

                    break;
                default:
                    break;
            }
        }

        // We only have an ID to work off of, so we have no validation as to the current state of the event.
        return scheduledGuildEventEditModel.Status.HasValue && scheduledGuildEventEditModel.Status.Value is ScheduledGuildEventStatus.Scheduled
            ? throw new ArgumentException("Status cannot be set to scheduled.")
            : ApiClient.ModifyScheduledGuildEventAsync(
            guildId,
            eventId,
            scheduledGuildEventEditModel.Name,
            scheduledGuildEventEditModel.Description,
            scheduledGuildEventEditModel.Channel.IfPresent(c => c?.Id),
            scheduledGuildEventEditModel.StartTime,
            scheduledGuildEventEditModel.EndTime,
            scheduledGuildEventEditModel.Type,
            scheduledGuildEventEditModel.PrivacyLevel,
            scheduledGuildEventEditModel.Metadata,
            scheduledGuildEventEditModel.Status
        );
    }

    /// <summary>
    /// Gets the users interested in the guild event.
    /// </summary>
    /// <param name="guildId">The ID of the guild the event resides on.</param>
    /// <param name="eventId">The ID of the event.</param>
    /// <param name="limit">How many users to query.</param>
    /// <param name="after">Fetch users after this ID.</param>
    /// <param name="before">Fetch users before this ID.</param>
    /// <returns>The users interested in the event.</returns>
    public async Task<IReadOnlyList<DiscordUser>> GetScheduledGuildEventUsersAsync(ulong guildId, ulong eventId, int limit = 100, ulong? after = null, ulong? before = null)
    {
        ulong? last = null;
        bool isAfter = after != null;
        List<DiscordUser> users = new List<DiscordUser>();
        int remaining = limit;
        int lastCount;

        do
        {
            int fetchSize = remaining > 100 ? 100 : remaining;
            IReadOnlyList<DiscordUser> fetch = await ApiClient.GetScheduledGuildEventUsersAsync(
                guildId,
                eventId,
                true,
                fetchSize,
                !isAfter ? last ?? before : null,
                isAfter ? last ?? after : null
            );

            lastCount = fetch.Count;
            remaining -= lastCount;

            if (!isAfter)
            {
                users.AddRange(fetch);
                last = fetch.LastOrDefault()?.Id;
            }
            else
            {
                users.InsertRange(0, fetch);
                last = fetch.FirstOrDefault()?.Id;
            }
        }
        while (remaining > 0 && lastCount > 0);

        return users.AsReadOnly();
    }
}
