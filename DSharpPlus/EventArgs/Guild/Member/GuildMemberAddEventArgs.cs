using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildMemberAdded"/> event.
/// </summary>
public class GuildMemberAddEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the member that was added.
    /// </summary>
    public DiscordMember Member { get; internal set; }

    /// <summary>
    /// Gets the guild the member was added to. This value is null if the guild wasn't cached.
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }
    
    /// <summary>
    /// Gets the id of the guild the member was added to.
    /// </summary>
    public ulong GuildId { get; internal set; }

    internal GuildMemberAddEventArgs() : base() { }
}
