using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments to  the WebhooksUpdated event.
/// </summary>
public class WebhooksUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild that had its webhooks updated.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the channel to which the webhook belongs to.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    internal WebhooksUpdatedEventArgs() : base() { }
}
