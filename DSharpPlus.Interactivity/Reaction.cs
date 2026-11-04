using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity;

/// <summary>
/// Represents a collection of reactions to a message.
/// </summary>
public sealed class Reaction
{
    /// <summary>
    /// The emoji that was reacted with.
    /// </summary>
    public DiscordEmoji Emoji { get; internal init; }

    /// <summary>
    /// The users who reacted with this emoji.
    /// </summary>
    public IReadOnlyList<DiscordUser> Users { get; internal init; }

    internal Reaction(DiscordEmoji emoji, IReadOnlyList<DiscordUser> users)
    {
        this.Emoji = emoji;
        this.Users = users;
    }
}
