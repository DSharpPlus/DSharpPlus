
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;
/// <summary>
/// Represents arguments for <see cref="DiscordClient.ThreadDeleted"/> event.
/// </summary>
public class ThreadDeleteEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the thread that was deleted.
    /// </summary>
    public DiscordThreadChannel Thread { get; internal set; }

    /// <summary>
    /// Gets the threads parent channel.
    /// </summary>
    public DiscordChannel Parent { get; internal set; }

    /// <summary>
    /// Gets the guild this thread belonged to.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal ThreadDeleteEventArgs() : base() { }
}
