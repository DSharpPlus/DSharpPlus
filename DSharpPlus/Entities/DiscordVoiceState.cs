using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord voice state.
    /// </summary>
    public class DiscordVoiceState
    {
        internal DiscordClient Discord { get; set; }

        /// <summary>
        /// Gets ID of the guild this voice state is associated with.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong? GuildId { get; set; }

        /// <summary>
        /// Gets the guild associated with this voice state.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild => this.GuildId != null ? this.Discord.Guilds[this.GuildId.Value] : null;

        /// <summary>
        /// Gets ID of the channel this user is connected to.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong? ChannelId { get; set; }

        /// <summary>
        /// Gets the channel this user is connected to.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel => this.ChannelId != null && this.ChannelId.Value != 0 ? this.Discord.InternalGetCachedChannel(this.ChannelId.Value) : null;

        /// <summary>
        /// Gets ID of the user to which this voice state belongs.
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
        /// Gets ID of the session of this voice state.
        /// </summary>
        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionId { get; internal set; }

        /// <summary>
        /// Gets whether this user is deafened.
        /// </summary>
        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool Deaf { get; internal set; }

        /// <summary>
        /// Gets whether this user is muted.
        /// </summary>
        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool Mute { get; internal set; }

        /// <summary>
        /// Gets whether this user is locally deafened.
        /// </summary>
        [JsonProperty("self_deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfDeaf { get; internal set; }

        /// <summary>
        /// Gets whether this user is locally muted.
        /// </summary>
        [JsonProperty("self_mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfMute { get; internal set; }

        /// <summary>
        /// Gets whether the current user has suppressed this user.
        /// </summary>
        [JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
        public bool Suppress { get; internal set; }

        public override string ToString()
        {
            return $"{this.UserId.ToString(CultureInfo.InvariantCulture)} in {this.GuildId?.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
