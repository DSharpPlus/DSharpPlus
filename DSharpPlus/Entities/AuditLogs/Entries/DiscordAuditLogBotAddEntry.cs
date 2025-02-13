namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents a auditlog entry for one of the following action types:
/// BotAdd
/// </summary>
public sealed class DiscordAuditLogBotAddEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the bot that has been added to the guild.
    /// </summary>
    public DiscordUser TargetBot { get; internal set; } = default!;
}
