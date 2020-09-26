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
		public DiscordGuild Guild
			=> this.GuildId != null ? this.Discord.Guilds[this.GuildId.Value] : (this.Channel != null ? this.Channel.Guild : null);

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
			=> this.ChannelId != null && this.ChannelId.Value != 0 ? this.Discord.InternalGetCachedChannel(this.ChannelId.Value) : null;

		/// <summary>
		/// Gets ID of the user to which this voice state belongs.
		/// </summary>
		[JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
		internal ulong UserId { get; set; }

		/// <summary>
		/// Gets the user associated with this voice state.
		/// <para>This can be cast to a <see cref="DiscordMember"/> if this voice state was in a guild.</para>
		/// </summary>
		[JsonIgnore]
		public DiscordUser User
		{
			get
			{
				var usr = null as DiscordUser;

				if (this.Guild != null)
					usr = this.Guild._members.TryGetValue(this.UserId, out var member) ? member : null;

				if (usr == null)
					usr = this.Discord.GetCachedOrEmptyUserInternal(this.UserId);

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
		public bool IsServerDeafened { get; internal set; }

		/// <summary>
		/// Gets whether this user is muted.
		/// </summary>
		[JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
		public bool IsServerMuted { get; internal set; }

		/// <summary>
		/// Gets whether this user is locally deafened.
		/// </summary>
		[JsonProperty("self_deaf", NullValueHandling = NullValueHandling.Ignore)]
		public bool IsSelfDeafened { get; internal set; }

		/// <summary>
		/// Gets whether this user is locally muted.
		/// </summary>
		[JsonProperty("self_mute", NullValueHandling = NullValueHandling.Ignore)]
		public bool IsSelfMuted { get; internal set; }

		/// <summary>
		/// Gets whether the current user has suppressed this user.
		/// </summary>
		[JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
		public bool IsSuppressed { get; internal set; }

		internal DiscordVoiceState() { }

		// copy constructor for reduced boilerplate
		internal DiscordVoiceState(DiscordVoiceState other)
		{
			this.Discord = other.Discord;

			this.UserId = other.UserId;
			this.ChannelId = other.ChannelId;
			this.GuildId = other.GuildId;

			this.IsServerDeafened = other.IsServerDeafened;
			this.IsServerMuted = other.IsServerMuted;
			this.IsSuppressed = other.IsSuppressed;
			this.IsSelfDeafened = other.IsSelfDeafened;
			this.IsSelfMuted = other.IsSelfMuted;

			this.SessionId = other.SessionId;
		}

		internal DiscordVoiceState(DiscordMember m)
		{
			this.Discord = m.Discord as DiscordClient;

			this.UserId = m.Id;
			this.ChannelId = 0;
			this.GuildId = m._guild_id;

			this.IsServerDeafened = m.IsDeafened;
			this.IsServerMuted = m.IsMuted;

			// Values not filled out are values that are not known from a DiscordMember
		}

		public override string ToString()
		{
			return $"{this.UserId.ToString(CultureInfo.InvariantCulture)} in {(this.GuildId ?? this.Channel.GuildId).ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
