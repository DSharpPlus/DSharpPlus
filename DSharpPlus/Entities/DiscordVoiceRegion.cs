using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents information about a Discord voice server region.
    /// </summary>
    public class DiscordVoiceRegion
    {
        /// <summary>
        /// Gets the unique ID for the region.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets an example server hostname for this region.
        /// </summary>
        [JsonProperty("sample_hostname", NullValueHandling = NullValueHandling.Ignore)]
        public string SampleHostname { get; internal set; }

        /// <summary>
        /// Gets an example server port for this region.
        /// </summary>
        [JsonProperty("sample_port", NullValueHandling = NullValueHandling.Ignore)]
        public int SamplePort { get; internal set; }

        /// <summary>
        /// Gets whether this is a VIP-only region.
        /// </summary>
        [JsonProperty("vip", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsVIP { get; internal set; }

        /// <summary>
        /// Gets whether this region is the most optimal for the current user.
        /// </summary>
        [JsonProperty("optimal", NullValueHandling = NullValueHandling.Ignore)]
        public bool Optimal { get; internal set; }

        /// <summary>
        /// Gets whether this voice region is deprecated.
        /// </summary>
        [JsonProperty("deprecated", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsDeprecated { get; internal set; }

        /// <summary>
        /// Gets whether this is a custom voice region.
        /// </summary>
        [JsonProperty("custom", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsCustom { get; internal set; }
    }
}
