namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for SessionCreated event.
/// </summary>
public sealed class SessionCreatedEventArgs : DiscordEventArgs
{
    internal SessionCreatedEventArgs() : base() { }

    /// <summary>
    /// The ID of the shard this event occurred on.
    /// </summary>
    public int ShardId { get; internal set; }
}
