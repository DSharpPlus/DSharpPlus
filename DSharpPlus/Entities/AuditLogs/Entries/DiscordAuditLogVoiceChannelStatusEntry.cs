namespace DSharpPlus.Entities.AuditLogs;

/// <summary>
/// Represents an audit log entry retaining to a voice channel status being changed.
/// </summary>
public sealed class DiscordAuditLogVoiceChannelStatusEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// The voice channel affected by this.
    /// </summary>
    public DiscordChannel Target { get; internal set; }

    /// <summary>
    /// The status of this voice channel.
    /// </summary>
    public PropertyChange<string?> Status { get; internal set; }
}
