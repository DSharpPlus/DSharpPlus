using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalActivityEmoji
{
    /// <summary>
    /// The name of the emoji.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The id of the emoji.
    /// </summary>
    [JsonPropertyName("id")]
    public Optional<InternalSnowflake> Id { get; init; }

    /// <summary>
    /// Whether this emoji is animated.
    /// </summary>
    [JsonPropertyName("animated")]
    public Optional<bool> Animated { get; init; }
}
