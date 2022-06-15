using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordVoiceRegion
    {
        /// <summary>
        /// The unique id for the region.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; init; } = null!;

        /// <summary>
        /// The name of the region.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// <see langword="true"/> for a single server that is closest to the current user's client.
        /// </summary>
        [JsonProperty("optimal", NullValueHandling = NullValueHandling.Ignore)]
        public bool Optimal { get; init; }

        /// <summary>
        /// Whether this is a deprecated voice region (avoid switching to these).
        /// </summary>
        [JsonProperty("deprecated", NullValueHandling = NullValueHandling.Ignore)]
        public bool Deprecated { get; init; }

        /// <summary>
        /// Whether this is a custom voice region (used for events/etc).
        /// </summary>
        [JsonProperty("custom", NullValueHandling = NullValueHandling.Ignore)]
        public bool Custom { get; init; }
    }
}
