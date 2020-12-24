using DSharpPlus.VoiceNext.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.VideoNext.Entities
{
    internal sealed class VideoSelectProtocolPayload
    {
        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("data")]
        public VideoSelectProtocolPayloadData Data { get; set; }
        
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("port")]
        public ushort Port { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }
        
        [JsonProperty("codecs")]
        public JArray Codecs { get; set; }
    }
}