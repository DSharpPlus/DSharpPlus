using DSharpPlus.Caching;

namespace DSharpPlus.Entities.AuditLogs;

/// <inheritdoc cref="DiscordAuditLogEntry"/>
public sealed class DiscordAuditLogGuildEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected guild.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild> Target { get; internal set; }

    /// <summary>
    /// Gets the description of guild name's change.
    /// </summary>
    public PropertyChange<string>? NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of owner's change.
    /// </summary>
    public PropertyChange<CachedEntity<ulong,DiscordMember>>? OwnerChange { get; internal set; }

    /// <summary>
    /// Gets the description of icon's change.
    /// </summary>
    public PropertyChange<string>? IconChange { get; internal set; }

    /// <summary>
    /// Gets the description of verification level's change.
    /// </summary>
    public PropertyChange<VerificationLevel>? VerificationLevelChange { get; internal set; }

    /// <summary>
    /// Gets the description of afk channel's change.
    /// </summary>
    public PropertyChange<CachedEntity<ulong, DiscordChannel>>? AfkChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of widget channel's change.
    /// </summary>
    public PropertyChange<CachedEntity<ulong, DiscordChannel>>? EmbedChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of notification settings' change.
    /// </summary>
    public PropertyChange<DefaultMessageNotifications>? NotificationSettingsChange { get; internal set; }

    /// <summary>
    /// Gets the description of system message channel's change.
    /// </summary>
    public PropertyChange<CachedEntity<ulong, DiscordChannel>>? SystemChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of explicit content filter settings' change.
    /// </summary>
    public PropertyChange<ExplicitContentFilter>? ExplicitContentFilterChange { get; internal set; }

    /// <summary>
    /// Gets the description of guild's mfa level change.
    /// </summary>
    public PropertyChange<MfaLevel>? MfaLevelChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite splash's change.
    /// </summary>
    public PropertyChange<string>? SplashChange { get; internal set; }

    /// <summary>
    /// Gets the description of the guild's region change.
    /// </summary>
    public PropertyChange<string>? RegionChange { get; internal set; }

    internal DiscordAuditLogGuildEntry() { }
}
