namespace DSharpPlus.EventArgs;

using DSharpPlus.Entities;

/// <summary>
/// Represents arguments to <see cref="DiscordClient.WebhooksUpdated"/> event.
/// </summary>
public class WebhooksUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild that had its webhooks updated.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the channel to which the webhook belongs to.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    internal WebhooksUpdateEventArgs() : base() { }
}
