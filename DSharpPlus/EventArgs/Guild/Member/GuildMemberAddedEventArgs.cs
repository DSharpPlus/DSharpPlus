using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for GuildMemberAdded event.
/// </summary>
public class GuildMemberAddedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the member that was added.
    /// </summary>
    public DiscordMember Member { get; internal set; }

    /// <summary>
    /// Gets the guild the member was added to.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal GuildMemberAddedEventArgs() : base() { }
}
