using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.VoiceEntities
{
    internal sealed class VoiceStateUpdatePayload
    {
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public string GuildId { get; set; }

        [JsonProperty("channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionId { get; set; }

        [JsonProperty("deaf")]
        public bool Deafened { get; set; }

        [JsonProperty("mute")]
        public bool Muted { get; set; }
    }
}
