using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalEmbedProvider
{
    /// <summary>
    /// The name of provider.
    /// </summary>
    [JsonPropertyName("name")]
    public Optional<string> Name { get; init; }

    /// <summary>
    /// The url of provider.
    /// </summary>
    [JsonPropertyName("url")]
    public Optional<string> Url { get; init; }
}
