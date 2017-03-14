using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.VoiceEntities
{
    internal sealed class VoiceSelectProtocolPayload
    {
        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("data")]
        public VoiceSelectProtocolPayloadData Data { get; set; }
    }
}
