using Newtonsoft.Json;

namespace DSharpPlus.VideoNext.Entities
{
    internal sealed class VideoIdentifyPayload
    {
        [JsonProperty("server_id")]
        public string ServerId { get; set; }

        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Video {get; set; } = null;
    }
}