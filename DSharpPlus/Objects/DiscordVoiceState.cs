using Newtonsoft.Json;

namespace DSharpPlus.Objects {
    /// <summary>
    /// The voice state of a user
    /// </summary>
    public class DiscordVoiceState {
        /// <summary>
        /// Server ID
        /// </summary>
        [JsonProperty("guild_id")]
        public string GuildID { get; internal set; }

        /// <summary>
        /// Channel ID
        /// </summary>
        [JsonProperty("channel_id")]
        public string ChannelID { get; internal set; }

        /// <summary>
        /// User ID
        /// </summary>
        [JsonProperty("user_id")]
        public string UserID { get; internal set; }

        /// <summary>
        /// Session ID
        /// </summary>
        [JsonProperty("session_id")]
        public string SessionID { get; internal set; }

        /// <summary>
        /// Wether this user has been deafened
        /// </summary>
        [JsonProperty("deaf")]
        public bool Deaf { get; internal set; }

        /// <summary>
        /// Wether this user has been muted
        /// </summary>
        [JsonProperty("mute")]
        public bool Mute { get; internal set; }

        /// <summary>
        /// Wether this user has deafened itself
        /// </summary>
        [JsonProperty("self_deaf")]
        public bool SelfDeaf { get; internal set; }

        /// <summary>
        /// Wether this user has muted itself
        /// </summary>
        [JsonProperty("self_mute")]
        public bool SelfMute { get; internal set; }

        /// <summary>
        /// Wether this user has been muted by you
        /// </summary>
        [JsonProperty("suppress")]
        public bool Suppress { get; internal set; }
    }
}
