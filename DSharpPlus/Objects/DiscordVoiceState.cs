using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordVoiceState
    {
        /// <summary>
        /// The guild id this voice state is for
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? GuildID { get; internal set; }
        /// <summary>
        /// The channel id this user is connected to
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelID { get; internal set; }
        /// <summary>
        /// The user id this voice state is for
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong UserID { get; internal set; }
        /// <summary>
        /// The session id for this voice state
        /// </summary>
        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionID { get; internal set; }
        /// <summary>
        /// Whether this user is deafened by the server
        /// </summary>
        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool Deaf { get; internal set; }
        /// <summary>
        /// Whether this user is muted by the server
        /// </summary>
        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool Mute { get; internal set; }
        /// <summary>
        /// Whether this user is locally deafened
        /// </summary>
        [JsonProperty("self_deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfDeaf { get; internal set; }
        /// <summary>
        /// Whether this user is locally muted
        /// </summary>
        [JsonProperty("self_mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfMute { get; internal set; }
        /// <summary>
        /// Whether this user is muted by the current user
        /// </summary>
        [JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
        public bool Suppress { get; internal set; }
    }
}
