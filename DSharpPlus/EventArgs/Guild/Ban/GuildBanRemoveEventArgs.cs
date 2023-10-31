using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildBanRemoved"/> event.
/// </summary>
public class GuildBanRemoveEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the member that just got unbanned.
    /// </summary>
    public DiscordMember Member { get; internal set; }

    /// <summary>
    /// Gets the guild this member was unbanned in. This value is null if the guild wasn't cached.
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }
    
    /// <summary>
    /// Gets the id of the guild this member was unbanned in.
    /// </summary>
    public ulong GuildId { get; internal set; }

    internal GuildBanRemoveEventArgs() : base() { }
}
