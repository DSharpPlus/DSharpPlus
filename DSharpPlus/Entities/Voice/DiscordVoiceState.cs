using System;
using System.Globalization;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Caching;

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
    /// Gets ID of the channel this user is connected to.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Include)]
    internal ulong? ChannelId { get; set; }

    /// <summary>
    /// Gets ID of the user to which this voice state belongs.
    /// </summary>
    [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong UserId { get; set; }

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
        this.TransportMember = other.TransportMember;
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

    public override string ToString() => $"{this.UserId.ToString(CultureInfo.InvariantCulture)} in channel {ChannelId ?? 0}";

    /// <summary>
    /// Gets the guild this voicestate belongs to.
    /// <para>Setting <paramref name="withCounts"/> to true will always make a REST request.</para>
    /// </summary>
    /// <param name="withCounts">Whether to include approximate presence and member counts in the returned guild.</param>
    /// <param name="skipCache">Whether to skip the cache and always excute a REST request</param>
    /// <returns>The requested Guild.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the voicestate does not belong the the guild</exception>
    public ValueTask<DiscordGuild> GetGuildAsync(bool skipCache = false, bool withCounts = false)
    {
        if (!GuildId.HasValue)
        {
            throw new InvalidOperationException("Voicestate does not belong to a guild");
        }
        return Discord.GetGuildAsync(GuildId.Value, withCounts, skipCache);
    }

    /// <summary>
    /// Gets the channel this voicestate belongs to.
    /// </summary>
    /// <param name="skipCache">Whether to skip the cache and always excute a REST request</param>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <exception cref="InvalidOperationException">This exception is only listed because Discord documents a voice state without a channel being possible, but not when or why</exception>
    public ValueTask<DiscordChannel> GetChannelAsync(bool skipCache = false)
    {
        if (!ChannelId.HasValue)
        {
            throw new InvalidOperationException("Voicestate does not belong to a guild");
        }

        return Discord.GetChannelAsync(ChannelId.Value, skipCache);
    }

    /// <summary>
    /// Gets the user to which this voicestate belongs
    /// </summary>
    /// <param name="skipCache">Whether to skip the cache and always excute a REST request</param>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public ValueTask<DiscordUser> GetUserAsync(bool skipCache = false)
        => Discord.GetUserAsync(UserId, skipCache);
    
    /// <summary>
    /// Gets the member to which this voicestate belongs
    /// </summary>
    /// <param name="skipCache">Whether to skip the cache and always excute a REST request</param>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async ValueTask<DiscordMember> GetMemberAsync(bool skipCache = false)
    {
        if (!GuildId.HasValue)
        {
            throw new InvalidOperationException("Voicestate does not belong to a guild");
        }

        if (!skipCache)
        {
            DiscordMember? member = await this.Discord.Cache.TryGetMemberAsync(this.UserId, this.GuildId.Value);
            if (member is not null)
            {
                return member;
            }
        }

        return await Discord.ApiClient.GetGuildMemberAsync(GuildId.Value, UserId);
    }
}
