using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for the GuildBanAdded event.
/// </summary>
public class GuildBanAddedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the member that was banned.
    /// </summary>
    public DiscordMember Member { get; internal set; }

    /// <summary>
    /// Gets the guild this member was banned in.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal GuildBanAddedEventArgs() : base() { }
}
