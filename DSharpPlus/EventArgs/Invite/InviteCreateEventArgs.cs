using DSharpPlus.Caching;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.InviteCreated"/>.
/// </summary>
public sealed class InviteCreateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild that created the invite.
    /// </summary>
    public CachedEntity<ulong, DiscordGuild> Guild { get; internal set; }

    /// <summary>
    /// Gets the channel that the invite is for. This value is null if the channel wasn't cached.
    /// </summary>
    public CachedEntity<ulong, DiscordChannel> Channel { get; internal set; }

    /// <summary>
    /// Gets the created invite.
    /// </summary>
    public DiscordInvite Invite { get; internal set; }

    internal InviteCreateEventArgs() : base() { }
}
