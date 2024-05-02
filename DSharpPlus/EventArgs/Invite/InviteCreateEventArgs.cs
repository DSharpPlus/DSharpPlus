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
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// Gets the channel that the invite is for.
    /// </summary>
    public DiscordChannel Channel { get; internal set; }

    /// <summary>
    /// Gets the created invite.
    /// </summary>
    public DiscordInvite Invite { get; internal set; }

    internal InviteCreateEventArgs() : base() { }
}
