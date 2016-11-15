using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordVoiceRegion : SnowflakeObject
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("sample_hostname")]
        public string SampleHostname { get; internal set; }
        [JsonProperty("sample_port")]
        public int SamplePort { get; internal set; }
        [JsonProperty("vip")]
        public bool VIP { get; internal set; }
        [JsonProperty("optimal")]
        public bool Optimal { get; internal set; }
        [JsonProperty("deprecated")]
        public bool Deprecated { get; internal set; }
        [JsonProperty("custom")]
        public bool Custom { get; internal set; }
    }
}
