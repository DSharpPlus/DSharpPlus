namespace DSharpPlus.EventArgs;

/// <summary>
/// Contains information sent with "resumed" events.
/// </summary>
public sealed class SessionResumedEventArgs : DiscordEventArgs
{
    internal SessionResumedEventArgs() : base() { }

    /// <summary>
    /// The ID of the shard this event occurred on.
    /// </summary>
    public int ShardId { get; internal set; }
}
