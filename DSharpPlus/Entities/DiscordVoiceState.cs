using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord voice state.
    /// </summary>
    public class DiscordVoiceState : PropertyChangedBase
    {
        private bool _isServerDeafened;
        private bool _isServerMuted;
        private bool _isSelfDeafened;
        private bool _isSelfMuted;

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
        public DiscordGuild Guild
            => GuildId != null ? Discord.Guilds[GuildId.Value] : (Channel != null ? Channel.Guild : null);

        /// <summary>
        /// Gets ID of the channel this user is connected to.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Include)]
        internal ulong? ChannelId { get; set; }

        /// <summary>
        /// Gets the channel this user is connected to.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel
            => ChannelId != null && ChannelId.Value != 0 ? Discord.InternalGetCachedChannel(ChannelId.Value) : null;

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

                if (Guild != null)
                {
                    usr = Guild._members.FirstOrDefault(xm => xm.Id == UserId);
                }

                if (usr == null)
                {
                    usr = Discord.InternalGetCachedUser(UserId);
                }

                return usr;
            }
        }

        /// <summary>
        /// Gets ID of the session of this voice state.
        /// </summary>
        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        internal string SessionId { get; set; }

        /// <summary>
        /// Gets whether this user is deafened.
        /// </summary>
        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsServerDeafened { get => _isServerDeafened; internal set => OnPropertySet(ref _isServerDeafened, value); }

        /// <summary>
        /// Gets whether this user is muted.
        /// </summary>
        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsServerMuted { get => _isServerMuted; internal set => OnPropertySet(ref _isServerMuted, value); }

        /// <summary>
        /// Gets whether this user is locally deafened.
        /// </summary>
        [JsonProperty("self_deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsSelfDeafened { get => _isSelfDeafened; internal set => OnPropertySet(ref _isSelfDeafened, value); }

        /// <summary>
        /// Gets whether this user is locally muted.
        /// </summary>
        [JsonProperty("self_mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsSelfMuted { get => _isSelfMuted; internal set => OnPropertySet(ref _isSelfMuted, value); }

        /// <summary>
        /// Gets whether the current user has suppressed this user.
        /// </summary>
        [JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsSuppressed { get; internal set; }

        internal DiscordVoiceState() { }

        // copy constructor for reduced boilerplate
        internal DiscordVoiceState(DiscordVoiceState other)
        {
            Discord = other.Discord;

            UserId = other.UserId;
            ChannelId = other.ChannelId;
            GuildId = other.GuildId;

            IsServerDeafened = other.IsServerDeafened;
            IsServerMuted = other.IsServerMuted;
            IsSuppressed = other.IsSuppressed;
            IsSelfDeafened = other.IsSelfDeafened;
            IsSelfMuted = other.IsSelfMuted;

            SessionId = other.SessionId;
        }

        internal DiscordVoiceState(DiscordMember m)
        {
            Discord = m.Discord as DiscordClient;

            UserId = m.Id;
            ChannelId = 0;
            GuildId = m._guild_id;

            IsServerDeafened = m.IsDeafened;
            IsServerMuted = m.IsMuted;

            // Values not filled out are values that are not known from a DiscordMember
        }

        public override string ToString()
        {
            return $"{UserId.ToString(CultureInfo.InvariantCulture)} in {(GuildId ?? Channel.GuildId).ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
