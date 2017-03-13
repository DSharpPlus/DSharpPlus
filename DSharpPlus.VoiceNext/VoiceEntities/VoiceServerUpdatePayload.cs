using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.VoiceEntities
{
    internal class VoiceServerUpdatePayload
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("guild_id")]
        public string GuildId { get; set; }

        [JsonProperty("endpoint")]
        public string EndPoint { get; set; }
    }
}
