using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// A partial channel object
/// </summary>
/// <remarks>
/// Partial objects can have any or no data, but the ID is always returned
/// </remarks>
public class DiscordPartialChannel
{
    /// <summary>
    /// Gets the ID of this object.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong Id { get; internal set; }

    /// <summary>
    /// Gets ID of the guild to which this channel belongs.
    /// </summary>
    [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? GuildId { get; internal set; }

    /// <summary>
    /// Gets ID of the category that contains this channel. For threads, gets the ID of the channel this thread was created in.
    /// </summary>
    [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Include)]
    public ulong? ParentId { get; internal set; }

    /// <summary>
    /// Gets the name of this channel.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name { get; internal set; }

    /// <summary>
    /// Gets the type of this channel.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordChannelType? Type { get; internal set; }

    /// <summary>
    /// Gets the position of this channel.
    /// </summary>
    [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
    public int? Position { get; internal set; }

    /// <summary>
    /// Gets a list of permission overwrites
    /// </summary>
    [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
    public List<DiscordOverwrite>? PermissionOverwrites = [];

    /// <summary>
    /// Gets the channel's topic. This is applicable to text channels only.
    /// </summary>
    [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
    public string? Topic { get; internal set; }

    /// <summary>
    /// Gets the ID of the last message sent in this channel. This is applicable to text channels only.
    /// </summary>
    /// <remarks>
    /// For forum posts, this ID may point to an invalid message (e.g. the OP deleted the initial forum message).
    /// </remarks>
    [JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? LastMessageId { get; internal set; }

    /// <summary>
    /// Gets this channel's bitrate. This is applicable to voice channels only.
    /// </summary>
    [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
    public int? Bitrate { get; internal set; }

    /// <summary>
    /// Gets this channel's user limit. This is applicable to voice channels only.
    /// </summary>
    [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
    public int? UserLimit { get; internal set; }

    /// <summary>
    /// <para>Gets the slow mode delay configured for this channel.</para>
    /// <para>All bots, as well as users with <see cref="DiscordPermission.ManageChannels"/> or <see cref="DiscordPermission.ManageMessages"/> permissions in the channel are exempt from slow mode.</para>
    /// </summary>
    [JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
    public int? PerUserRateLimit { get; internal set; }

    /// <summary>
    /// Gets this channel's video quality mode. This is applicable to voice channels only.
    /// </summary>
    [JsonProperty("video_quality_mode", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordVideoQualityMode? QualityMode { get; internal set; }

    /// <summary>
    /// Gets when the last pinned message was pinned.
    /// </summary>
    [JsonProperty("last_pin_timestamp", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? LastPinTimestamp { get; internal set; }

    /// <summary>
    /// Gets whether this channel is an NSFW channel.
    /// </summary>
    [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsNsfw { get; internal set; }

    /// <summary>
    /// Get the name of the voice region
    /// </summary>
    [JsonProperty("rtc_region", NullValueHandling = NullValueHandling.Ignore)]
    public string? RtcRegionId { get; set; }

    /// <summary>
    /// Gets the permissions of the user who invoked the command in this channel.
    /// <para>Only sent on the resolved channels of interaction responses for application commands.</para>
    /// </summary>
    [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordPermissions? UserPermissions { get; internal set; }

    internal DiscordPartialChannel()
    {
    }
}
