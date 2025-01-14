namespace DSharpPlus.Net.InboundWebhooks;

/// <summary>
/// Represents the type of the webhook event.
/// </summary>
public enum DiscordWebhookEventType
{
    /// <summary>
    /// A ping event.
    /// </summary>
    Ping = 0,

    /// <summary>
    /// An event.
    /// </summary>
    Event = 1,
}
