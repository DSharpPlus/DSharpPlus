using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.VoiceEntities
{
    internal class VoiceStateUpdatePayload
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }
    }
}
