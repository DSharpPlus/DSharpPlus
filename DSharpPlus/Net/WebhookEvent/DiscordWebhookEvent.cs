namespace DSharpPlus.Net;

/// <summary>
/// A wrapper type for an incoming webhook event.
/// </summary>
public sealed class DiscordWebhookEvent
{
    /// <summary>
    /// Gets the version.
    /// </summary>
    public int Version { get; internal set; }

    /// <summary>
    /// Gets the ID of the application that triggered this event.
    /// </summary>
    public ulong ApplicationID { get; internal set; }

    /// <summary>
    /// Gets the type of the event.
    /// </summary>
    public DiscordWebhookEventType Type { get; internal set; }
}
