using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildBanAdded"/> event.
/// </summary>
public class GuildBanAddEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the member that was banned.
    /// </summary>
    public DiscordMember Member { get; internal set; }

    /// <summary>
    /// Gets the guild this member was banned in. This value is null if the guild wasn't cached.
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }
    
    /// <summary>
    /// Gets the id of the guild this member was banned in.
    /// </summary>
    public ulong GuildId { get; internal set; }

    internal GuildBanAddEventArgs() : base() { }
}
