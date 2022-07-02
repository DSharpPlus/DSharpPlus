using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordVoiceRegion
    {
        /// <summary>
        /// The unique id for the region.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; init; } = null!;

        /// <summary>
        /// The name of the region.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// <see langword="true"/> for a single server that is closest to the current user's client.
        /// </summary>
        [JsonPropertyName("optimal")]
        public bool Optimal { get; init; }

        /// <summary>
        /// Whether this is a deprecated voice region (avoid switching to these).
        /// </summary>
        [JsonPropertyName("deprecated")]
        public bool Deprecated { get; init; }

        /// <summary>
        /// Whether this is a custom voice region (used for events/etc).
        /// </summary>
        [JsonPropertyName("custom")]
        public bool Custom { get; init; }
    }
}
