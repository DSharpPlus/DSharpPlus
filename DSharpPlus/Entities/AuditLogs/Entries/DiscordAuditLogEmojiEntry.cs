namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogEmojiEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected emoji.
    /// </summary>
    public DiscordEmoji Target { get; internal set; }

    /// <summary>
    /// Gets the description of emoji's name change.
    /// </summary>
    public PropertyChange<string> NameChange { get; internal set; }

    internal DiscordAuditLogEmojiEntry() { }
}
