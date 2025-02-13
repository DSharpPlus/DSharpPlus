using System.Collections.Generic;

namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// ChannelCreate, ChannelDelete, ChannelUpdate
/// </summary>
public sealed class DiscordAuditLogChannelEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected channel.
    /// </summary>
    public DiscordChannel Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of channel's name change.
    /// </summary>
    public PropertyChange<string> NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel's type change.
    /// </summary>
    public PropertyChange<DiscordChannelType?> TypeChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel's nsfw flag change.
    /// </summary>
    public PropertyChange<bool?> NsfwChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel's bitrate change.
    /// </summary>
    public PropertyChange<int?> BitrateChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel permission overwrites' change.
    /// </summary>
    public PropertyChange<IReadOnlyList<DiscordOverwrite>> OverwriteChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel's topic change.
    /// </summary>
    public PropertyChange<string> TopicChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel's slow mode timeout change.
    /// </summary>
    public PropertyChange<int?> PerUserRateLimitChange { get; internal set; }

    public PropertyChange<int?> UserLimit { get; internal set; }

    public PropertyChange<DiscordChannelFlags?> Flags { get; internal set; }

    public PropertyChange<IEnumerable<DiscordForumTag>> AvailableTags { get; internal set; }

    internal DiscordAuditLogChannelEntry() { }
}
