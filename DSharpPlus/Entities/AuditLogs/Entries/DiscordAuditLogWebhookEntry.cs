namespace DSharpPlus.Entities.AuditLogs;

using Caching;

public sealed class DiscordAuditLogWebhookEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected webhook.
    /// </summary>
    public DiscordWebhook Target { get; internal set; }

    /// <summary>
    /// Gets the description of webhook's name change.
    /// </summary>
    public PropertyChange<string> NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of webhook's target channel change.
    /// </summary>
    public PropertyChange<CachedEntity<ulong, DiscordChannel>?>? ChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of webhook's type change.
    /// </summary>
    public PropertyChange<int?> TypeChange { get; internal set; }

    /// <summary>
    /// Gets the description of webhook's avatar change.
    /// </summary>
    public PropertyChange<string> AvatarHashChange { get; internal set; }

    /// <summary>
    /// Gets the change in application ID.
    /// </summary>
    public PropertyChange<ulong?> ApplicationIdChange { get; internal set; }

    internal DiscordAuditLogWebhookEntry() { }
}
