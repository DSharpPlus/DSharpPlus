
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;
/// <summary>
/// Represents arguments for <see cref="DiscordClient.ThreadCreated"/> event.
/// </summary>
public class ThreadCreateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets whether this thread has been newly created.
    /// </summary>
    public bool NewlyCreated { get; internal set; }

    /// <summary>
    /// Gets the thread that was created.
    /// </summary>
    public DiscordThreadChannel Thread { get; internal set; }

    /// <summary>
    /// Gets the threads parent channel.
    /// </summary>
    public DiscordChannel Parent { get; internal set; }

    /// <summary>
    /// Gets the guild in which the thread was created.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal ThreadCreateEventArgs() : base() { }
}
