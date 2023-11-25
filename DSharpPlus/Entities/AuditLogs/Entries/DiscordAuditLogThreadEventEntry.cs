namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogThreadEventEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the target thread.
    /// </summary>
    public DiscordThreadChannel Target { get; internal set; }

    /// <summary>
    /// Gets a change in the thread's name.
    /// </summary>
    public PropertyChange<string?> Name { get; internal set; }

    /// <summary>
    /// Gets a change in channel type.
    /// </summary>
    public PropertyChange<ChannelType?> Type { get; internal set; }

    /// <summary>
    /// Gets a change in the thread's archived status.
    /// </summary>
    public PropertyChange<bool?> Archived { get; internal set; }

    /// <summary>
    /// Gets a change in the thread's auto archive duration.
    /// </summary>
    public PropertyChange<int?> AutoArchiveDuration { get; internal set; }

    /// <summary>
    /// Gets a change in the threads invitibility
    /// </summary>
    public PropertyChange<bool?> Invitable { get; internal set; }

    /// <summary>
    /// Gets a change in the thread's locked status
    /// </summary>
    public PropertyChange<bool?> Locked { get; internal set; }

    /// <summary>
    /// Gets a change in the thread's slowmode setting
    /// </summary>
    public PropertyChange<int?> PerUserRateLimit { get; internal set; }

    /// <summary>
    /// Gets a change in channel flags
    /// </summary>
    public PropertyChange<ChannelFlags?> Flags { get; internal set; }

    internal DiscordAuditLogThreadEventEntry() { }
}
