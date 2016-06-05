using Newtonsoft.Json;

namespace DiscordSharp.Objects {
    public class DiscordVoiceState {
        // guild_id		snowflake?	the guild id this voice state is for
        [JsonProperty("guild_id")]
        public string GuildID { get; internal set; }

        // channel_id	snowflake	the channel id this user is connected to
        [JsonProperty("channel_id")]
        public string ChannelID { get; internal set; }

        // user_id		snowflake	the user id this voice state is for
        [JsonProperty("user_id")]
        public string UserID { get; internal set; }

        // session_id	string		the session id for this voice state
        [JsonProperty("session_id")]
        public string SessionID { get; internal set; }

        // deaf			bool		whether this user is deafened by the server
        [JsonProperty("deaf")]
        public bool Deaf { get; internal set; }

        // mute			bool		whether this user is muted by the server
        [JsonProperty("mute")]
        public bool Mute { get; internal set; }

        // self_deaf	bool		whether this user is locally deafened
        [JsonProperty("self_deaf")]
        public bool SelfDeaf { get; internal set; }

        // self_mute	bool		whether this user is locally muted
        [JsonProperty("self_mute")]
        public bool SelfMute { get; internal set; }

        // suppress		bool		whether this user is muted by the current user
        [JsonProperty("suppress")]
        public bool Suppress { get; internal set; }
    }
}
