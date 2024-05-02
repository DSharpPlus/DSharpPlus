using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an emoji used for reacting to a forum post.
/// </summary>
public sealed class DefaultReaction
{
    /// <summary>
    /// The ID of the emoji, if applicable.
    /// </summary>
    [JsonProperty("emoji_id")]
    public ulong? EmojiId { get; internal set; }

    /// <summary>
    /// The unicode emoji, if applicable.
    /// </summary>
    [JsonProperty("emoji_name")]
    public string? EmojiName { get; internal set; }

    /// <summary>
    /// Creates a DefaultReaction object from an emoji.
    /// </summary>
    /// <param name="emoji">The <see cref="DiscordEmoji"/>.</param>
    /// <returns>Create <see cref="DefaultReaction"/> object.</returns>
    public static DefaultReaction FromEmoji(DiscordEmoji emoji) => emoji.Id == 0
            ? new DefaultReaction { EmojiName = emoji.Name }
            : new DefaultReaction { EmojiId = emoji.Id };
}
