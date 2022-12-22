using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalActivityEmoji
{
    /// <summary>
    /// The name of the emoji.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The id of the emoji.
    /// </summary>
    [JsonPropertyName("id")]
    public Optional<Snowflake> Id { get; init; }

    /// <summary>
    /// Whether this emoji is animated.
    /// </summary>
    [JsonPropertyName("animated")]
    public Optional<bool> Animated { get; init; }
}
