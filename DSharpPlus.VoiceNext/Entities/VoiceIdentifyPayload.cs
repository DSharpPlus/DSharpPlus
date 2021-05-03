using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.Entities
{
    internal sealed class VoiceIdentifyPayload
    {
        [JsonProperty("server_id")]
        public ulong ServerId { get; set; }

        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? UserId { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
