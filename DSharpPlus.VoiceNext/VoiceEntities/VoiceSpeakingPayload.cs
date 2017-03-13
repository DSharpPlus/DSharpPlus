using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.VoiceEntities
{
    internal class VoiceSpeakingPayload
    {
        [JsonProperty("speaking")]
        public bool Speaking { get; set; }

        [JsonProperty("delay")]
        public int Delay { get; set; }
    }
}
