
using Newtonsoft.Json;

namespace DSharpPlus.Entities;
/// <summary>
/// Represents a reaction to a message.
/// </summary>
public class DiscordReaction
{
    /// <summary>
    /// Gets the total number of users who reacted with this emoji.
    /// </summary>
    [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
    public int Count { get; internal set; }

    /// <summary>
    /// Gets whether the current user reacted with this emoji.
    /// </summary>
    [JsonProperty("me", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsMe { get; internal set; }

    /// <summary>
    /// Gets the emoji used to react to this message.
    /// </summary>
    [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordEmoji Emoji { get; internal set; } = default!;

    internal DiscordReaction() { }
}
