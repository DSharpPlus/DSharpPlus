using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Fired when a poll completes and results are available.
/// </summary>
public sealed class MessagePollCompletedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The message containing the poll results.
    /// </summary>
    public DiscordPollCompletionMessage PollCompletion { get; internal set; }

    /// <summary>
    /// Gets the channel this message belongs to.
    /// </summary>
    public DiscordChannel Channel
        => this.PollCompletion.Channel;

    /// <summary>
    /// Gets the guild this message belongs to.
    /// </summary>
    public DiscordGuild Guild
        => this.Channel.Guild;
}
