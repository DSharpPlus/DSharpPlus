namespace DSharpPlus.Net.Models;

using System;
using System.IO;

using DSharpPlus.Entities;

public class ScheduledGuildEventEditModel : BaseEditModel
{
    /// <summary>
    /// The new name of the event.
    /// </summary>
    public Optional<string> Name { get; set; }

    /// <summary>
    /// The new description of the event.
    /// </summary>
    public Optional<string> Description { get; set; }

    /// <summary>
    /// The new channel ID of the event. This must be set to null for external events.
    /// </summary>
    public Optional<DiscordChannel?> Channel { get; set; }

    /// <summary>
    /// The new privacy of the event.
    /// </summary>
    public Optional<DiscordScheduledGuildEventPrivacyLevel> PrivacyLevel { get; set; }

    /// <summary>
    /// The type of the event.
    /// </summary>
    public Optional<DiscordScheduledGuildEventType> Type { get; set; }

    /// <summary>
    /// The new time of the event.
    /// </summary>
    public Optional<DateTimeOffset> StartTime { get; set; }

    /// <summary>
    /// The new end time of the event.
    /// </summary>
    public Optional<DateTimeOffset> EndTime { get; set; }

    /// <summary>
    /// The new metadata of the event.
    /// </summary>
    public Optional<DiscordScheduledGuildEventMetadata> Metadata { get; set; }

    /// <summary>
    /// The new status of the event.
    /// </summary>
    public Optional<DiscordScheduledGuildEventStatus> Status { get; set; }

    /// <summary>
    /// The cover image for this event.
    /// </summary>
    public Optional<Stream> CoverImage { get; set; }

    internal ScheduledGuildEventEditModel() { }
}
