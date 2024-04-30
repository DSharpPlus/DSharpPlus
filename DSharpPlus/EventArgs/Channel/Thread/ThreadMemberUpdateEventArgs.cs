namespace DSharpPlus.EventArgs;

using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.ThreadMemberUpdated"/> event.
/// </summary>
public class ThreadMemberUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the thread member that was updated.
    /// </summary>
    public DiscordThreadChannelMember ThreadMember { get; internal set; }

    /// <summary>
    /// Gets the thread the current member was updated for.
    /// </summary>
    public DiscordThreadChannel Thread { get; internal set; }

    internal ThreadMemberUpdateEventArgs() : base() { }
}
