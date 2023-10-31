using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.InviteDeleted"/>
/// </summary>
public sealed class InviteDeleteEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild that deleted the invite. This value is null if the guild wasn't cached.
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }
    
    /// <summary>
    /// Gets the id of the guild that deleted the invite.
    /// </summary>
    public ulong GuildId { get; internal set; }

    /// <summary>
    /// Gets the channel that the invite was for. This value is null if the channel wasn't cached.
    /// </summary>
    public DiscordChannel? Channel { get; internal set; }
    
    /// <summary>
    /// Gets the id of the channel that the invite was for.
    /// </summary>
    public ulong ChannelId { get; internal set; }

    /// <summary>
    /// Gets the deleted invite.
    /// </summary>
    public DiscordInvite Invite { get; internal set; }

    internal InviteDeleteEventArgs() : base() { }
}
