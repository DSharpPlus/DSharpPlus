using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.VoiceEntities
{
    internal sealed class VoiceIdentifyPayload
    {
        [JsonProperty("server_id")]
        public string ServerId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
