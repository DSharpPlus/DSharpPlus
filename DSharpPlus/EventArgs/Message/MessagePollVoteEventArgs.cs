using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents an update for a poll vote.
/// </summary>
public sealed class MessagePollVoteEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the vote update.
    /// </summary>
    public DiscordPollVoteUpdate PollVoteUpdate { get; internal set; }
}
