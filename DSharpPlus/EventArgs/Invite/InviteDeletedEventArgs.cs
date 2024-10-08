using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for InviteDeleted
/// </summary>
public sealed class InviteDeletedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild that deleted the invite.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the channel that the invite was for.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    /// <summary>
    /// Gets the deleted invite.
    /// </summary>
    public DiscordInvite Invite { get; internal set; }

    internal InviteDeletedEventArgs() : base() { }
}
