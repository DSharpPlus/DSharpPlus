using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord voice state.
/// </summary>
public class DiscordVoiceState
{
    [JsonIgnore]
    internal BaseDiscordClient Discord { get; set; }

    /// <summary>
    /// Gets ID of the guild this voice state is associated with.
    /// </summary>
    [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? GuildId { get; init; }

    /// <summary>
    /// Gets ID of the channel this user is connected to.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Include)]
    public ulong? ChannelId { get; init; }

    /// <summary>
    /// Gets ID of the user to which this voice state belongs.
    /// </summary>
    [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong UserId { get; init; }

    /// <summary>
    /// Gets ID of the session of this voice state.
    /// </summary>
    [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
    public string SessionId { get; internal init; }

    /// <summary>
    /// Gets whether this user is deafened.
    /// </summary>
    [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsServerDeafened { get; internal init; }

    /// <summary>
    /// Gets whether this user is muted.
    /// </summary>
    [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsServerMuted { get; internal init; }

    /// <summary>
    /// Gets whether this user is locally deafened.
    /// </summary>
    [JsonProperty("self_deaf", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSelfDeafened { get; internal init; }

    /// <summary>
    /// Gets whether this user is locally muted.
    /// </summary>
    [JsonProperty("self_mute", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSelfMuted { get; internal init; }

    /// <summary>
    /// Gets whether this user's camera is enabled.
    /// </summary>
    [JsonProperty("self_video", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSelfVideo { get; internal init; }

    /// <summary>
    /// Gets whether this user is using the Go Live feature.
    /// </summary>
    [JsonProperty("self_stream", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSelfStream { get; internal init; }

    /// <summary>
    /// Gets whether the current user has suppressed this user.
    /// </summary>
    [JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsSuppressed { get; internal init; }

    /// <summary>
    /// Gets the time at which this user requested to speak.
    /// </summary>
    [JsonProperty("request_to_speak_timestamp", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? RequestToSpeakTimestamp { get; internal init; }

    [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
    internal TransportMember TransportMember { get; init; }
    
    /// <summary>
    /// Gets the guild associated with this voice state.
    /// </summary>
    /// <returns>Returns the guild associated with this voicestate</returns>
    public async ValueTask<DiscordGuild?> GetGuildAsync(bool skipCache = false)
    {
        if (this.GuildId is null)
        {
            return null;
        }
        
        if (skipCache)
        {
            return await this.Discord.ApiClient.GetGuildAsync(this.GuildId.Value, false);
        }

        if (this.Discord.Guilds.TryGetValue(this.GuildId.Value, out DiscordGuild? guild))
        {
            return guild;
        }

        guild = await this.Discord.ApiClient.GetGuildAsync(this.GuildId.Value, false);

        if (this.Discord is DiscordClient dc)
        {
            dc.guilds.TryAdd(this.GuildId.Value, guild);
        } 
            
        return guild;
    }
    
    /// <summary>
    /// Gets the member associated with this voice state.
    /// </summary>
    /// <param name="skipCache">Whether to skip the cache and always fetch the member from the API.</param>
    /// <returns>Returns the member associated with this voice state. Null if the voice state is not associated with a guild.</returns>
    public async ValueTask<DiscordUser?> GetUserAsync(bool skipCache = false)
    {
        if (this.GuildId is null)
        {
            return null;
        }

        if (skipCache)
        {
            return await this.Discord.ApiClient.GetGuildMemberAsync(this.GuildId.Value, this.UserId);
        }
        
        DiscordGuild? guild = await GetGuildAsync(skipCache);

        if (guild is null)
        {
            return null;
        }
        
        if (guild.Members.TryGetValue(this.UserId, out DiscordMember? member))
        {
            return member;
        }

        member = new DiscordMember(this.TransportMember) { Discord = this.Discord };

        if (this.Discord is DiscordClient dc)
        {
            dc.guilds.TryAdd(this.GuildId.Value, guild);
        }

        return member;
    }
    
    /// <summary>
    /// Gets the channel associated with this voice state.
    /// </summary>
    /// <param name="skipCache">Whether to skip the cache and always fetch the channel from the API.</param>
    /// <returns>Returns the channel associated with this voice state. Null if the voice state is not associated with a guild.</returns>
    public async ValueTask<DiscordChannel?> GetChannelAsync(bool skipCache = false)
    {
        if (this.ChannelId is null || this.GuildId is null)
        {
            return null;
        }

        if (skipCache)
        {
            return await this.Discord.ApiClient.GetChannelAsync(this.ChannelId.Value);
        }
        
        DiscordGuild? guild = await GetGuildAsync(skipCache);

        if (guild is null)
        {
            return null;
        }
        
        if (guild.Channels.TryGetValue(this.ChannelId.Value, out DiscordChannel? channel))
        {
            return channel;
        }

        channel = await this.Discord.ApiClient.GetChannelAsync(this.ChannelId.Value);

        if (this.Discord is DiscordClient dc)
        {
            dc.guilds.TryAdd(this.GuildId.Value, guild);
        }

        return channel;
    }

    internal DiscordVoiceState() { }
    internal DiscordVoiceState(DiscordMember member)
    {
        this.Discord = (DiscordClient)member.Discord;
        this.UserId = member.Id;
        this.ChannelId = 0;
        this.GuildId = member.guild_id;
        this.IsServerDeafened = member.IsDeafened;
        this.IsServerMuted = member.IsMuted;

        // Values not filled out are values that are not known from a DiscordMember
    }

    public override string ToString() => $"{this.UserId.ToString(CultureInfo.InvariantCulture)} in {this.GuildId?.ToString(CultureInfo.InvariantCulture)}";
}
