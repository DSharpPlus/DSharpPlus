namespace DSharpPlus.EventArgs;
using System.Collections.Generic;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.MessageCreated"/> event.
/// </summary>
public class MessageCreateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the message that was created.
    /// </summary>
    public DiscordMessage Message { get; internal set; }

    /// <summary>
    /// Gets the channel this message belongs to.
    /// </summary>
    public DiscordChannel Channel
        => Message.Channel;

    /// <summary>
    /// Gets the guild this message belongs to.
    /// </summary>
    public DiscordGuild Guild
        => Channel.Guild;

    /// <summary>
    /// Gets the author of the message.
    /// </summary>
    public DiscordUser Author
        => Message.Author;

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

    internal MessageCreateEventArgs() : base() { }
}
