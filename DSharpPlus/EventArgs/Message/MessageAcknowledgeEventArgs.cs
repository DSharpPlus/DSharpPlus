using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessageAcknowledged"/> event.
/// </summary>
public sealed class MessageAcknowledgeEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the message that was acknowledged.
    /// </summary>
    public DiscordMessage Message { get; internal set; }

    /// <summary>
    /// Gets the channel for which the message was acknowledged.
    /// </summary>
    public DiscordChannel Channel
        => this.Message.Channel;

    internal MessageAcknowledgeEventArgs() : base() { }
}
