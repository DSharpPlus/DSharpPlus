using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordVoiceRegion
    {
        /// <summary>
        /// Unique ID for the region
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; internal set; }
        /// <summary>
        /// Name of the region
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }
        /// <summary>
        /// An example hostname for the region
        /// </summary>
        [JsonProperty("sample_hostname")]
        public string SampleHostname { get; internal set; }
        /// <summary>
        /// An example port for the region
        /// </summary>
        [JsonProperty("sample_port")]
        public int SamplePort { get; internal set; }
        /// <summary>
        /// true if this is a vip-only server
        /// </summary>
        [JsonProperty("vip")]
        public bool VIP { get; internal set; }
        /// <summary>
        /// true for a single server that is closest to the current user's client
        /// </summary>
        [JsonProperty("optimal")]
        public bool Optimal { get; internal set; }
        /// <summary>
        /// Whether this is a deprecated voice region
        /// </summary>
        [JsonProperty("deprecated")]
        public bool Deprecated { get; internal set; }
        /// <summary>
        /// Whether this is a custom voice region
        /// </summary>
        [JsonProperty("custom")]
        public bool Custom { get; internal set; }
    }
}
