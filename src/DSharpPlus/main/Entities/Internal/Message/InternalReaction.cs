using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalReaction
{
    /// <summary>
    /// Times this emoji has been used to react.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }

    /// <summary>
    /// Whether the current user reacted using this emoji.
    /// </summary>
    [JsonPropertyName("me")]
    public bool Me { get; init; }

    /// <summary>
    /// The emoji information.
    /// </summary>
    [JsonPropertyName("emoji")]
    public InternalEmoji Emoji { get; init; } = null!;
}
