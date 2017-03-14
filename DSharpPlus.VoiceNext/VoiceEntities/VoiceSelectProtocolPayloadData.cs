using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.VoiceEntities
{
    internal class VoiceSelectProtocolPayloadData
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("port")]
        public ushort Port { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }
    }
}
