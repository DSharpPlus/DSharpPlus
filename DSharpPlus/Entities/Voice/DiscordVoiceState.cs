using System;
using System.Globalization;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

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
        => this.GuildId != null ? this.Discord.Guilds[this.GuildId.Value] : this.Channel?.Guild;

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
            DiscordUser? usr = null as DiscordUser;

            if (this.Guild != null)
            {
                usr = this.Guild._members.TryGetValue(this.UserId, out DiscordMember? member) ? member : null;
            }

            if (usr == null)
            {
                usr = this.Discord.GetCachedOrEmptyUserInternal(this.UserId);
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
    /// Gets whether this user's camera is enabled.
    /// </summary>
    [JsonProperty("self_video", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSelfVideo { get; internal set; }

    /// <summary>
    /// Gets whether this user is using the Go Live feature.
    /// </summary>
    [JsonProperty("self_stream", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSelfStream { get; internal set; }

    /// <summary>
    /// Gets whether the current user has suppressed this user.
    /// </summary>
    [JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSuppressed { get; internal set; }

    /// <summary>
    /// Gets the time at which this user requested to speak.
    /// </summary>
    [JsonProperty("request_to_speak_timestamp", NullValueHandling = NullValueHandling.Ignore)]
    internal DateTimeOffset? RequestToSpeakTimestamp { get; set; }

    /// <summary>
    /// Gets the member this voice state belongs to.
    /// </summary>
    [JsonIgnore]
    public DiscordMember Member
        => this.Guild.Members.TryGetValue(this.TransportMember.User.Id, out DiscordMember? member) ? member : new DiscordMember(this.TransportMember) { Discord = this.Discord };

    [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
    internal TransportMember TransportMember { get; set; }

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
        this.IsSelfStream = other.IsSelfStream;
        this.IsSelfVideo = other.IsSelfVideo;

        this.SessionId = other.SessionId;
        this.RequestToSpeakTimestamp = other.RequestToSpeakTimestamp;
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

    public override string ToString() => $"{this.UserId.ToString(CultureInfo.InvariantCulture)} in {(this.GuildId ?? this.Channel.GuildId.Value).ToString(CultureInfo.InvariantCulture)}";
}
