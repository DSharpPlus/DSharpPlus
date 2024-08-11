using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for ThreadUpdated event.
/// </summary>
public class ThreadUpdatedEventArgs : DiscordEventArgs
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

    internal ThreadUpdatedEventArgs() : base() { }
}
