namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// MessagePin, MessageUnpin
/// </summary>
public sealed class DiscordAuditLogMessagePinEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected message's user.
    /// </summary>
    public DiscordUser Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the channel the message is in.
    /// </summary>
    public DiscordChannel Channel { get; internal set; } = default!;

    /// <summary>
    /// Gets the message the pin action was for.
    /// </summary>
    public DiscordMessage Message { get; internal set; } = default!;

    internal DiscordAuditLogMessagePinEntry() { }
}
