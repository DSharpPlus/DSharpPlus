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
<<<<<<< HEAD
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("id")]
>>>>>>> master
        public string ID { get; internal set; }
        /// <summary>
        /// Name of the region
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("name")]
>>>>>>> master
        public string Name { get; internal set; }
        /// <summary>
        /// An example hostname for the region
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("sample_hostname", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("sample_hostname")]
>>>>>>> master
        public string SampleHostname { get; internal set; }
        /// <summary>
        /// An example port for the region
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("sample_port", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("sample_port")]
>>>>>>> master
        public int SamplePort { get; internal set; }
        /// <summary>
        /// true if this is a vip-only server
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("vip", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("vip")]
>>>>>>> master
        public bool VIP { get; internal set; }
        /// <summary>
        /// true for a single server that is closest to the current user's client
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("optimal", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("optimal")]
>>>>>>> master
        public bool Optimal { get; internal set; }
        /// <summary>
        /// Whether this is a deprecated voice region
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("deprecated", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("deprecated")]
>>>>>>> master
        public bool Deprecated { get; internal set; }
        /// <summary>
        /// Whether this is a custom voice region
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("custom", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("custom")]
>>>>>>> master
        public bool Custom { get; internal set; }
    }
}
