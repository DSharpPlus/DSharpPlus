using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a Discord voice state.
    /// </summary>
    public class DiscordVoiceState
    {
        internal DiscordClient Discord { get; set; }

        /// <summary>
        /// The guild id this voice state is for
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong? GuildId { get; set; }

        /// <summary>
        /// Gets the guild associated with this voice state.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild => this.GuildId != null ? this.Discord.Guilds[this.GuildId.Value] : null;

        /// <summary>
        /// The channel id this user is connected to
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong? ChannelId { get; set; }

        /// <summary>
        /// Gets the voice channel associated with this voice state.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel => this.ChannelId != null && this.ChannelId.Value != 0 ? this.Discord.InternalGetCachedChannel(this.ChannelId.Value) : null;

        /// <summary>
        /// The user id this voice state is for
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong UserId { get; set; }

        /// <summary>
        /// Gets the user associated with this voice state.
        /// </summary>
        [JsonIgnore]
        public DiscordUser User
        {
            get
            {
                var usr = null as DiscordUser;

                if (this.Guild != null)
                    usr = this.Guild._members.FirstOrDefault(xm => xm.Id == this.UserId);

                if (usr == null)
                    usr = this.Discord.InternalGetCachedUser(this.UserId);

                return usr;
            }
        }

        /// <summary>
        /// The session id for this voice state
        /// </summary>
        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionId { get; internal set; }

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

        public override string ToString()
        {
            return $"{this.UserId} in {this.GuildId}";
        }
    }
}
