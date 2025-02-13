namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// GuildUpdate
/// </summary>
public sealed class DiscordAuditLogGuildEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected guild.
    /// </summary>
    public DiscordGuild Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of guild name's change.
    /// </summary>
    public PropertyChange<string?> NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of owner's change.
    /// </summary>
    public PropertyChange<DiscordMember?> OwnerChange { get; internal set; }

    /// <summary>
    /// Gets the description of icon's change.
    /// </summary>
    public PropertyChange<string?> IconChange { get; internal set; }

    /// <summary>
    /// Gets the description of verification level's change.
    /// </summary>
    public PropertyChange<DiscordVerificationLevel?> VerificationLevelChange { get; internal set; }

    /// <summary>
    /// Gets the description of afk channel's change.
    /// </summary>
    public PropertyChange<DiscordChannel?> AfkChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of widget channel's change.
    /// </summary>
    public PropertyChange<DiscordChannel?> EmbedChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of notification settings' change.
    /// </summary>
    public PropertyChange<DiscordDefaultMessageNotifications?> NotificationSettingsChange { get; internal set; }

    /// <summary>
    /// Gets the description of system message channel's change.
    /// </summary>
    public PropertyChange<DiscordChannel?> SystemChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of explicit content filter settings' change.
    /// </summary>
    public PropertyChange<DiscordExplicitContentFilter?> ExplicitContentFilterChange { get; internal set; }

    /// <summary>
    /// Gets the description of guild's mfa level change.
    /// </summary>
    public PropertyChange<DiscordMfaLevel?> MfaLevelChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite splash's change.
    /// </summary>
    public PropertyChange<string?> SplashChange { get; internal set; }

    /// <summary>
    /// Gets the description of the guild's region change.
    /// </summary>
    public PropertyChange<string?> RegionChange { get; internal set; }

    internal DiscordAuditLogGuildEntry() { }
}
