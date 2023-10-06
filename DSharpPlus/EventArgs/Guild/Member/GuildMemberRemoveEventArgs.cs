using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildMemberRemoved"/> event.
/// </summary>
public class GuildMemberRemoveEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild the member was removed from.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the member that was removed.
    /// </summary>
    public DiscordMember Member { get; internal set; }

    internal GuildMemberRemoveEventArgs() : base() { }
}
