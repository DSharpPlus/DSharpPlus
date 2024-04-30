namespace DSharpPlus.EventArgs;

using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ThreadUpdated"/> event.
/// </summary>
public class ThreadUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the post-update thread.
    /// </summary>
    public DiscordThreadChannel ThreadAfter { get; internal set; }

    /// <summary>
    /// Gets the pre-update thread.
    /// </summary>
    public DiscordThreadChannel ThreadBefore { get; internal set; }

    /// <summary>
    /// Gets the threads parent channel.
    /// </summary>
    public DiscordChannel Parent { get; internal set; }

    /// <summary>
    /// Gets the guild in which the thread was updated.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal ThreadUpdateEventArgs() : base() { }
}
