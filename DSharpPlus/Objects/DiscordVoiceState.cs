using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordVoiceState
    {
        [JsonProperty("guild_id")]
        public ulong? GuildID { get; internal set; }
        [JsonProperty("channel_id")]
        public ulong ChannelID { get; internal set; }
        [JsonProperty("user_id")]
        public ulong UserID { get; internal set; }
        [JsonProperty("session_id")]
        public string SessionID { get; internal set; }
        [JsonProperty("deaf")]
        public bool Deaf { get; internal set; }
        [JsonProperty("mute")]
        public bool Mute { get; internal set; }
        [JsonProperty("self_deaf")]
        public bool SelfDeaf { get; internal set; }
        [JsonProperty("self_mute")]
        public bool SelfMute { get; internal set; }
        [JsonProperty("suppress")]
        public bool Suppress { get; internal set; }
    }
}
