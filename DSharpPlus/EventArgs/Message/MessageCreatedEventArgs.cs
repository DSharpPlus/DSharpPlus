using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for MessageCreated event.
/// </summary>
public class MessageCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the message that was created.
    /// </summary>
    public DiscordMessage Message { get; internal set; }

    /// <summary>
    /// Gets the channel this message belongs to.
    /// </summary>
    public DiscordChannel Channel
        => this.Message.Channel;

    /// <summary>
    /// Gets the guild this message belongs to.
    /// </summary>
    public DiscordGuild Guild
        => this.Channel.Guild;

    /// <summary>
    /// Gets the author of the message.
    /// </summary>
    public DiscordUser Author
        => this.Message.Author;

    /// <summary>
    /// Gets the collection of mentioned users.
    /// </summary>
    public IReadOnlyList<DiscordUser> MentionedUsers { get; internal set; }

    /// <summary>
    /// Gets the collection of mentioned roles.
    /// </summary>
    public IReadOnlyList<DiscordRole> MentionedRoles { get; internal set; }

    /// <summary>
    /// Gets the collection of mentioned channels.
    /// </summary>
    public IReadOnlyList<DiscordChannel> MentionedChannels { get; internal set; }

    internal MessageCreatedEventArgs() : base() { }
}
