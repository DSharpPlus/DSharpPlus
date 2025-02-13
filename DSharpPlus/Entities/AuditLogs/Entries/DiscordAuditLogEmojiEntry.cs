namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// EmojiCreate, EmojiDelete, EmojiUpdate
/// </summary>
public sealed class DiscordAuditLogEmojiEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected emoji.
    /// </summary>
    public DiscordEmoji Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of emoji's name change.
    /// </summary>
    public PropertyChange<string> NameChange { get; internal set; }

    internal DiscordAuditLogEmojiEntry() { }
}
