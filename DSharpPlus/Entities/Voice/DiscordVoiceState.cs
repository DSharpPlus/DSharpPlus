namespace DSharpPlus.Entities;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

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
    internal ulong? GuildId { get; init; }

    /// <summary>
    /// Gets the guild associated with this voice state.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild? Guild => GuildId is not null ? Discord.Guilds[GuildId.Value] : Channel?.Guild;

    /// <summary>
    /// Gets ID of the channel this user is connected to.
    /// </summary>
    [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Include)]
    internal ulong? ChannelId { get; init; }

    /// <summary>
    /// Gets the channel this user is connected to.
    /// </summary>
    [JsonIgnore]
    public DiscordChannel? Channel => (ChannelId.HasValue && ChannelId.Value != 0) ? Discord.InternalGetCachedChannel(ChannelId.Value) : null;

    /// <summary>
    /// Gets ID of the user to which this voice state belongs.
    /// </summary>
    [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong UserId { get; init; }

    /// <summary>
    /// Gets the user associated with this voice state.
    /// <para>This can be cast to a <see cref="DiscordMember"/> if this voice state was in a guild.</para>
    /// </summary>
    [JsonIgnore]
    public DiscordUser User => Guild is not null && Guild._members.TryGetValue(UserId, out DiscordMember? member)
                ? member
                : Discord.GetCachedOrEmptyUserInternal(UserId);

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

    /// <summary>
    /// Gets the member this voice state belongs to.
    /// </summary>
    [JsonIgnore, NotNullIfNotNull(nameof(Guild))]
    public DiscordMember? Member => Guild is not null && Guild.Members.TryGetValue(TransportMember.User.Id, out DiscordMember? member)
                ? member
                : new DiscordMember(TransportMember) { Discord = Discord };

    [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
    internal TransportMember TransportMember { get; init; }

    internal DiscordVoiceState() { }
    internal DiscordVoiceState(DiscordMember member)
    {
        Discord = (DiscordClient)member.Discord;
        UserId = member.Id;
        ChannelId = 0;
        GuildId = member._guild_id;
        IsServerDeafened = member.IsDeafened;
        IsServerMuted = member.IsMuted;

        // Values not filled out are values that are not known from a DiscordMember
    }

    public override string ToString() => $"{UserId.ToString(CultureInfo.InvariantCulture)} in {(GuildId ?? Channel?.GuildId)?.ToString(CultureInfo.InvariantCulture)}";
}
