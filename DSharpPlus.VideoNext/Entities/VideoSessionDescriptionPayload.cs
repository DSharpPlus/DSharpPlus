using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.VideoNext.Entities
{
    internal sealed class VideoSessionDescriptionPayload
    {
        [JsonProperty("video_codec")]
        public string Codec { get; set; }
        
        [JsonProperty("secret_key")]
        public byte[] SecretKey { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }
        
        [JsonProperty("media_session_id")]
        public string MediaSession { get; set; }
        
        [JsonProperty("encodings")] 
        public JArray Encodings { get; set; }
    }
}