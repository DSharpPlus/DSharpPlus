using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalVoiceRegion
{
    /// <summary>
    /// The unique id for the region.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// The name of the region.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// <see langword="true"/> for a single server that is closest to the current user's client.
    /// </summary>
    [JsonPropertyName("optimal")]
    public required bool Optimal { get; init; }

    /// <summary>
    /// Whether this is a deprecated voice region (avoid switching to these).
    /// </summary>
    [JsonPropertyName("deprecated")]
    public required bool Deprecated { get; init; }

    /// <summary>
    /// Whether this is a custom voice region (used for events/etc).
    /// </summary>
    [JsonPropertyName("custom")]
    public required bool Custom { get; init; }
}
