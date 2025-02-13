namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// GuildScheduledEventCreate, GuildScheduledEventDelete, GuildScheduledEventUpdate
/// </summary>
public sealed class DiscordAuditLogGuildScheduledEventEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets a change in the event's name
    /// </summary>
    public PropertyChange<string?> Name { get; internal set; }

    /// <summary>
    /// Gets the target event. Note that this will only have the ID specified if it is not cached.
    /// </summary>
    public DiscordScheduledGuildEvent Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the channel the event was changed to.
    /// </summary>
    public PropertyChange<DiscordChannel?> Channel { get; internal set; }

    /// <summary>
    /// Gets the description change of the event.
    /// </summary>
    public PropertyChange<string?> Description { get; internal set; }

    /// <summary>
    /// Gets the change of type for the event.
    /// </summary>
    public PropertyChange<DiscordScheduledGuildEventType?> Type { get; internal set; }

    /// <summary>
    /// Gets the change in image hash.
    /// </summary>
    public PropertyChange<string?> ImageHash { get; internal set; }

    /// <summary>
    /// Gets the change in event location, if it's an external event.
    /// </summary>
    public PropertyChange<string?> Location { get; internal set; }

    /// <summary>
    /// Gets change in privacy level.
    /// </summary>
    public PropertyChange<DiscordScheduledGuildEventPrivacyLevel?> PrivacyLevel { get; internal set; }

    /// <summary>
    /// Gets the change in status.
    /// </summary>
    public PropertyChange<DiscordScheduledGuildEventStatus?> Status { get; internal set; }

    public DiscordAuditLogGuildScheduledEventEntry() { }
}
