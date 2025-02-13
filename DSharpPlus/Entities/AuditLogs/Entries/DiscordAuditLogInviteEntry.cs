namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// InviteCreate, InviteDelete, InviteUpdate
/// </summary>
public sealed class DiscordAuditLogInviteEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected invite.
    /// </summary>
    public DiscordInvite Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of invite's max age change.
    /// </summary>
    public PropertyChange<int?> MaxAgeChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's code change.
    /// </summary>
    public PropertyChange<string> CodeChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's temporariness change.
    /// </summary>
    public PropertyChange<bool?> TemporaryChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's inviting member change.
    /// </summary>
    public PropertyChange<DiscordMember> InviterChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's target channel change.
    /// </summary>
    public PropertyChange<DiscordChannel> ChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's use count change.
    /// </summary>
    public PropertyChange<int?> UsesChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's max use count change.
    /// </summary>
    public PropertyChange<int?> MaxUsesChange { get; internal set; }

    internal DiscordAuditLogInviteEntry() { }
}
