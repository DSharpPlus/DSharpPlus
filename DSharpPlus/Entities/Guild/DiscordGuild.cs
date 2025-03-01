using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using DSharpPlus.Entities.AuditLogs;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord guild.
/// </summary>
public class DiscordGuild : SnowflakeObject, IEquatable<DiscordGuild>
{
    /// <summary>
    /// Gets the guild's name.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the guild icon's hash.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
    public string IconHash { get; internal set; }

    /// <summary>
    /// Gets the guild icon's url.
    /// </summary>
    [JsonIgnore]
    public string IconUrl
        => GetIconUrl(MediaFormat.Auto, 1024);

    /// <summary>
    /// Gets the guild splash's hash.
    /// </summary>
    [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
    public string SplashHash { get; internal set; }

    /// <summary>
    /// Gets the guild splash's url.
    /// </summary>
    [JsonIgnore]
    public string? SplashUrl
        => !string.IsNullOrWhiteSpace(this.SplashHash) ? $"https://cdn.discordapp.com/splashes/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.SplashHash}.jpg" : null;

    /// <summary>
    /// Gets the guild discovery splash's hash.
    /// </summary>
    [JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
    public string DiscoverySplashHash { get; internal set; }

    /// <summary>
    /// Gets the guild discovery splash's url.
    /// </summary>
    [JsonIgnore]
    public string? DiscoverySplashUrl
        => !string.IsNullOrWhiteSpace(this.DiscoverySplashHash) ? $"https://cdn.discordapp.com/discovery-splashes/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.DiscoverySplashHash}.jpg" : null;

    /// <summary>
    /// Gets the preferred locale of this guild.
    /// <para>This is used for server discovery and notices from Discord. Defaults to en-US.</para>
    /// </summary>
    [JsonProperty("preferred_locale", NullValueHandling = NullValueHandling.Ignore)]
    public string PreferredLocale { get; internal set; }

    /// <summary>
    /// Gets the ID of the guild's owner.
    /// </summary>
    [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong OwnerId { get; internal set; }

    /// <summary>
    /// Gets permissions for the user in the guild (does not include channel overrides)
    /// </summary>
    [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordPermissions? Permissions { get; set; }

    /// <summary>
    /// Gets the guild's owner.
    /// </summary>
    public async Task<DiscordMember> GetGuildOwnerAsync()
    {
        return this.Members.TryGetValue(this.OwnerId, out DiscordMember? owner)
            ? owner
            : await this.Discord.ApiClient.GetGuildMemberAsync(this.Id, this.OwnerId);
    }

    /// <summary>
    /// Gets the guild's voice region ID.
    /// </summary>
    [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
    internal string voiceRegionId { get; set; }

    /// <summary>
    /// Gets the guild's voice region.
    /// </summary>
    [JsonIgnore]
    public DiscordVoiceRegion VoiceRegion
        => this.Discord.VoiceRegions[this.voiceRegionId];

    /// <summary>
    /// Gets the guild's AFK voice channel ID.
    /// </summary>
    [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? AfkChannelId { get; internal set; }

    /// <summary>
    /// Gets the guild's AFK voice channel.
    /// </summary>
    /// <param name="skipCache">If set to true this method will skip all caches and always perform a rest api call</param>
    /// <returns>Returns null if the guild has no AFK channel</returns>
    public async Task<DiscordChannel?> GetAfkChannelAsync(bool skipCache = false)
    {
        if (this.AfkChannelId is null)
        {
            return null;
        }

        return await GetChannelAsync(this.AfkChannelId.Value);
    }

    /// <summary>
    /// Gets the guild's AFK timeout.
    /// </summary>
    [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
    public int AfkTimeout { get; internal set; }

    /// <summary>
    /// Gets the guild's verification level.
    /// </summary>
    [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordVerificationLevel VerificationLevel { get; internal set; }

    /// <summary>
    /// Gets the guild's default notification settings.
    /// </summary>
    [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordDefaultMessageNotifications DefaultMessageNotifications { get; internal set; }

    /// <summary>
    /// Gets the guild's explicit content filter settings.
    /// </summary>
    [JsonProperty("explicit_content_filter")]
    public DiscordExplicitContentFilter ExplicitContentFilter { get; internal set; }

    /// <summary>
    /// Gets the guild's nsfw level.
    /// </summary>
    [JsonProperty("nsfw_level")]
    public DiscordNsfwLevel NsfwLevel { get; internal set; }

    /// <summary>
    /// Id of the channel where system messages (such as boost and welcome messages) are sent.
    /// </summary>
    [JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Include)]
    public ulong? SystemChannelId { get; internal set; }

    /// <summary>
    /// Gets the channel where system messages (such as boost and welcome messages) are sent.
    /// </summary>
    /// <param name="skipCache">If set to true this method will skip all caches and always perform a rest api call</param>
    /// <returns>Returns null if the guild has no configured system channel.</returns>
    public async Task<DiscordChannel?> GetSystemChannelAsync(bool skipCache = false)
    {
        if (this.SystemChannelId is null)
        {
            return null;
        }

        return await GetChannelAsync(this.SystemChannelId.Value);
    }

    /// <summary>
    /// Gets the settings for this guild's system channel.
    /// </summary>
    [JsonProperty("system_channel_flags")]
    public DiscordSystemChannelFlags SystemChannelFlags { get; internal set; }

    /// <summary>
    /// Id of the channel where safety alerts are sent to
    /// </summary>
    [JsonProperty("safety_alerts_channel_id")]
    public ulong? SafetyAlertsChannelId { get; internal set; }

    /// <summary>
    /// Gets the guild's safety alerts channel.
    /// </summary>
    /// <param name="skipCache">If set to true this method will skip all caches and always perform a rest api call</param>
    ///<returns>Returns null if the guild has no configured safety alerts channel.</returns>
    public async Task<DiscordChannel?> GetSafetyAlertsChannelAsync(bool skipCache = false)
    {
        if (this.SafetyAlertsChannelId is null)
        {
            return null;
        }

        return await GetChannelAsync(this.SafetyAlertsChannelId.Value);
    }

    /// <summary>
    /// Gets whether this guild's widget is enabled.
    /// </summary>
    [JsonProperty("widget_enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool? WidgetEnabled { get; internal set; }

    /// <summary>
    /// Id of the widget channel
    /// </summary>
    [JsonProperty("widget_channel_id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? WidgetChannelId { get; internal set; }

    /// <summary>
    /// Gets the widget channel for this guild.
    /// </summary>
    /// <param name="skipCache">If set to true this method will skip all caches and always perform a rest api call</param>
    /// <returns>Returns null if the guild has no widget channel configured.</returns>
    public async Task<DiscordChannel?> GetWidgetChannelAsync(bool skipCache = false)
    {
        if (this.WidgetChannelId is null)
        {
            return null;
        }

        return await GetChannelAsync(this.WidgetChannelId.Value);
    }

    /// <summary>
    /// Id of the rules channel of this guild. Null if the guild has no configured rules channel.
    /// </summary>
    [JsonProperty("rules_channel_id")]
    public ulong? RulesChannelId { get; internal set; }

    /// <summary>
    /// Gets the rules channel for this guild.
    /// <para>This is only available if the guild is considered "discoverable".</para>
    /// </summary>
    /// <param name="skipCache">If set to true this method will skip all caches and always perform a rest api call</param>
    /// <returns>Returns null if the guild has no rules channel configured</returns>
    public async Task<DiscordChannel?> GetRulesChannelAsync(bool skipCache = false)
    {
        if (this.RulesChannelId is null)
        {
            return null;
        }

        return await GetChannelAsync(this.RulesChannelId.Value);
    }

    /// <summary>
    /// Id of the channel where admins and moderators receive messages from Discord
    /// </summary>
    [JsonProperty("public_updates_channel_id")]
    public ulong? PublicUpdatesChannelId { get; internal set; }

    /// <summary>
    /// Gets the public updates channel (where admins and moderators receive messages from Discord) for this guild.
    /// <para>This is only available if the guild is considered "discoverable".</para>
    /// </summary>
    /// <param name="skipCache">If set to true this method will skip all caches and always perform a rest api call</param>
    /// <returns>Returns null if the guild has no public updates channel configured</returns>
    public async Task<DiscordChannel?> GetPublicUpdatesChannelAsync(bool skipCache = false)
    {
        if (this.PublicUpdatesChannelId is null)
        {
            return null;
        }

        return await GetChannelAsync(this.PublicUpdatesChannelId.Value);
    }

    /// <summary>
    /// Gets the application ID of this guild if it is bot created.
    /// </summary>
    [JsonProperty("application_id")]
    public ulong? ApplicationId { get; internal set; }

    /// <summary>
    /// Scheduled events for this guild.
    /// </summary>
    public IReadOnlyDictionary<ulong, DiscordScheduledGuildEvent> ScheduledEvents
        => new ReadOnlyConcurrentDictionary<ulong, DiscordScheduledGuildEvent>(this.scheduledEvents);

    [JsonProperty("guild_scheduled_events")]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordScheduledGuildEvent> scheduledEvents = new();

    /// <summary>
    /// Gets a collection of this guild's roles.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordRole> Roles => new ReadOnlyConcurrentDictionary<ulong, DiscordRole>(this.roles);

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordRole> roles;

    /// <summary>
    /// Gets a collection of this guild's stickers.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordMessageSticker> Stickers => new ReadOnlyConcurrentDictionary<ulong, DiscordMessageSticker>(this.stickers);

    [JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordMessageSticker> stickers = new();

    /// <summary>
    /// Gets a collection of this guild's emojis.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordEmoji> Emojis => new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(this.emojis);

    [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordEmoji> emojis;

    /// <summary>
    /// Gets a collection of this guild's features.
    /// </summary>
    [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<string> Features { get; internal set; }

    /// <summary>
    /// Gets the required multi-factor authentication level for this guild.
    /// </summary>
    [JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordMfaLevel MfaLevel { get; internal set; }

    /// <summary>
    /// Gets this guild's join date.
    /// </summary>
    [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset JoinedAt { get; internal set; }

    /// <summary>
    /// Gets whether this guild is considered to be a large guild.
    /// </summary>
    [JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsLarge { get; internal set; }

    /// <summary>
    /// Gets whether this guild is unavailable.
    /// </summary>
    [JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsUnavailable { get; internal set; }

    /// <summary>
    /// Gets the total number of members in this guild.
    /// </summary>
    [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
    public int MemberCount { get; internal set; }

    /// <summary>
    /// Gets the maximum amount of members allowed for this guild.
    /// </summary>
    [JsonProperty("max_members")]
    public int? MaxMembers { get; internal set; }

    /// <summary>
    /// Gets the maximum amount of presences allowed for this guild.
    /// </summary>
    [JsonProperty("max_presences")]
    public int? MaxPresences { get; internal set; }

#pragma warning disable CS1734
    /// <summary>
    /// Gets the approximate number of members in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name="withCounts"></paramref> set to true.
    /// </summary>
    [JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
    public int? ApproximateMemberCount { get; internal set; }

    /// <summary>
    /// Gets the approximate number of presences in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name="withCounts"></paramref> set to true.
    /// </summary>
    [JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
    public int? ApproximatePresenceCount { get; internal set; }
#pragma warning restore CS1734

    /// <summary>
    /// Gets the maximum amount of users allowed per video channel.
    /// </summary>
    [JsonProperty("max_video_channel_users", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxVideoChannelUsers { get; internal set; }

    /// <summary>
    /// Gets a dictionary of all the voice states for this guilds. The key for this dictionary is the ID of the user
    /// the voice state corresponds to.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordVoiceState> VoiceStates => new ReadOnlyConcurrentDictionary<ulong, DiscordVoiceState>(this.voiceStates);

    [JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordVoiceState> voiceStates;

    /// <summary>
    /// Gets a dictionary of all the members that belong to this guild. The dictionary's key is the member ID.
    /// </summary>
    [JsonIgnore] // TODO overhead of => vs Lazy? it's a struct
    public IReadOnlyDictionary<ulong, DiscordMember> Members => new ReadOnlyConcurrentDictionary<ulong, DiscordMember>(this.members);

    [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordMember> members;

    /// <summary>
    /// Gets a dictionary of all the channels associated with this guild. The dictionary's key is the channel ID.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordChannel> Channels => new ReadOnlyConcurrentDictionary<ulong, DiscordChannel>(this.channels);

    [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordChannel> channels;

    /// <summary>
    /// Gets a dictionary of all the active threads associated with this guild the user has permission to view. The dictionary's key is the channel ID.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordThreadChannel> Threads => new ReadOnlyConcurrentDictionary<ulong, DiscordThreadChannel>(this.threads);

    [JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordThreadChannel> threads = new();

    internal ConcurrentDictionary<string, DiscordInvite> invites;

    /// <summary>
    /// Gets the guild member for current user.
    /// </summary>
    [JsonIgnore]
    public DiscordMember CurrentMember => this.members != null && this.members.TryGetValue(this.Discord.CurrentUser.Id, out DiscordMember? member) ? member : null;

    /// <summary>
    /// Gets the @everyone role for this guild.
    /// </summary>
    [JsonIgnore]
    public DiscordRole EveryoneRole
        => this.Roles.GetValueOrDefault(this.Id)!;

    [JsonIgnore]
    internal bool isOwner;

    /// <summary>
    /// Gets whether the current user is the guild's owner.
    /// </summary>
    [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsOwner
    {
        get => this.isOwner || this.OwnerId == this.Discord.CurrentUser.Id;
        internal set => this.isOwner = value;
    }

    /// <summary>
    /// Gets the vanity URL code for this guild, when applicable.
    /// </summary>
    [JsonProperty("vanity_url_code")]
    public string VanityUrlCode { get; internal set; }

    /// <summary>
    /// Gets the guild description, when applicable.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; internal set; }

    /// <summary>
    /// Gets this guild's banner hash, when applicable.
    /// </summary>
    [JsonProperty("banner")]
    public string Banner { get; internal set; }

    /// <summary>
    /// Gets this guild's banner in url form.
    /// </summary>
    [JsonIgnore]
    public string? BannerUrl
        => !string.IsNullOrWhiteSpace(this.Banner) ? $"https://cdn.discordapp.com/banners/{this.Id}/{this.Banner}" : null;

    /// <summary>
    /// Gets this guild's premium tier (Nitro boosting).
    /// </summary>
    [JsonProperty("premium_tier")]
    public DiscordPremiumTier PremiumTier { get; internal set; }

    /// <summary>
    /// Gets the amount of members that boosted this guild.
    /// </summary>
    [JsonProperty("premium_subscription_count", NullValueHandling = NullValueHandling.Ignore)]
    public int? PremiumSubscriptionCount { get; internal set; }

    /// <summary>
    /// Whether the guild has the boost progress bar enabled.
    /// </summary>
    [JsonProperty("premium_progress_bar_enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool PremiumProgressBarEnabled { get; internal set; }

    /// <summary>
    /// Gets whether this guild is designated as NSFW.
    /// </summary>
    [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsNSFW { get; internal set; }

    /// <summary>
    /// Gets the stage instances in this guild.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ulong, DiscordStageInstance> StageInstances => new ReadOnlyConcurrentDictionary<ulong, DiscordStageInstance>(this.stageInstances);

    [JsonProperty("stage_instances", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordStageInstance> stageInstances;

    // Failed attempts so far: 8
    // Velvet got it working in one attempt. I'm not mad, why would I be mad. - Lunar
    /// <summary>
    /// Gets channels ordered in a manner in which they'd be ordered in the UI of the discord client.
    /// </summary>
    [JsonIgnore]
    // Group the channels by category or parent id
    public IEnumerable<DiscordChannel> OrderedChannels => this.channels.Values.GroupBy(channel => channel.IsCategory ? channel.Id : channel.ParentId)
        // Order the channel by the category's position
        .OrderBy(channels => channels.FirstOrDefault(channel => channel.IsCategory)?.Position)
        // Select the category's channels
        // Order them by text, shoving voice or stage types to the bottom
        // Then order them by their position
        .Select(channel => channel.OrderBy(channel => channel.Type is DiscordChannelType.Voice or DiscordChannelType.Stage).ThenBy(channel => channel.Position))
        // Group them all back together into a single enumerable.
        .SelectMany(channel => channel);

    [JsonIgnore]
    internal bool isSynced { get; set; }

    internal DiscordGuild() => this.invites = new ConcurrentDictionary<string, DiscordInvite>();

    #region Guild Methods

    /// <summary>
    /// Gets guild's icon URL, in requested format and size.
    /// </summary>
    /// <param name="imageFormat">The image format of the icon to get.</param>
    /// <param name="imageSize">The maximum size of the icon. Must be a power of two, minimum 16, maximum 4096.</param>
    /// <returns>The URL of the guild's icon.</returns>
    public string? GetIconUrl(MediaFormat imageFormat, ushort imageSize = 1024)
    {

        if (string.IsNullOrWhiteSpace(this.IconHash))
        {
            return null;
        }

        if (imageFormat == MediaFormat.Unknown)
        {
            throw new ArgumentException("You must specify valid image format.", nameof(imageFormat));
        }

        // Makes sure the image size is in between Discord's allowed range.
        if (imageSize is < 16 or > 4096)
        {
            throw new ArgumentOutOfRangeException(nameof(imageSize), imageSize, "Image Size is not in between 16 and 4096.");
        }

        // Checks to see if the image size is not a power of two.
        if (!(imageSize is not 0 && (imageSize & (imageSize - 1)) is 0))
        {
            throw new ArgumentOutOfRangeException(nameof(imageSize), imageSize, "Image size is not a power of two.");
        }

        // Get the string variants of the method parameters to use in the urls.
        string stringImageFormat = imageFormat switch
        {
            MediaFormat.Gif => "gif",
            MediaFormat.Jpeg => "jpg",
            MediaFormat.Png => "png",
            MediaFormat.WebP => "webp",
            MediaFormat.Auto => !string.IsNullOrWhiteSpace(this.IconHash) ? this.IconHash.StartsWith("a_") ? "gif" : "png" : "png",
            _ => throw new ArgumentOutOfRangeException(nameof(imageFormat)),
        };
        string stringImageSize = imageSize.ToString(CultureInfo.InvariantCulture);

        return $"https://cdn.discordapp.com/{Endpoints.ICONS}/{this.Id}/{this.IconHash}.{stringImageFormat}?size={stringImageSize}";

    }

    /// <summary>
    /// Creates a new scheduled event in this guild.
    /// </summary>
    /// <param name="name">The name of the event to create, up to 100 characters.</param>
    /// <param name="description">The description of the event, up to 1000 characters.</param>
    /// <param name="channelId">If a <see cref="DiscordScheduledGuildEventType.StageInstance"/> or <see cref="DiscordScheduledGuildEventType.VoiceChannel"/>, the id of the channel the event will be hosted in</param>
    /// <param name="type">The type of the event. <see paramref="channelId"/> must be supplied if not an external event.</param>
    /// <param name="privacyLevel">The privacy level of thi</param>
    /// <param name="start">When this event starts. Must be in the future, and before the end date.</param>
    /// <param name="end">When this event ends. If supplied, must be in the future and after the end date. This is required for <see cref="DiscordScheduledGuildEventType.External"/>.</param>
    /// <param name="location">Where this event takes place, up to 100 characters. Only applicable if the type is <see cref="DiscordScheduledGuildEventType.External"/></param>
    /// <param name="image">A cover image for this event.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns>The created event.</returns>
    public async Task<DiscordScheduledGuildEvent> CreateEventAsync(string name, string description, ulong? channelId, DiscordScheduledGuildEventType type, DiscordScheduledGuildEventPrivacyLevel privacyLevel, DateTimeOffset start, DateTimeOffset? end, string? location = null, Stream? image = null, string? reason = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(start, DateTimeOffset.Now);
        if (end != null && end <= start)
        {
            throw new ArgumentOutOfRangeException(nameof(end), "The end time for an event must be after the start time.");
        }

        DiscordScheduledGuildEventMetadata? metadata = null;
        switch (type)
        {
            case DiscordScheduledGuildEventType.StageInstance or DiscordScheduledGuildEventType.VoiceChannel when channelId == null:
                throw new ArgumentException($"{nameof(channelId)} must not be null when type is {type}", nameof(channelId));
            case DiscordScheduledGuildEventType.External when channelId != null:
                throw new ArgumentException($"{nameof(channelId)} must be null when using external event type", nameof(channelId));
            case DiscordScheduledGuildEventType.External when location == null:
                throw new ArgumentException($"{nameof(location)} must not be null when using external event type", nameof(location));
            case DiscordScheduledGuildEventType.External when end == null:
                throw new ArgumentException($"{nameof(end)} must not be null when using external event type", nameof(end));
        }

        if (!string.IsNullOrEmpty(location))
        {
            metadata = new DiscordScheduledGuildEventMetadata()
            {
                Location = location
            };
        }

        return await this.Discord.ApiClient.CreateScheduledGuildEventAsync(this.Id, name, description, start, type, privacyLevel, metadata, end, channelId, image, reason);
    }

    /// <summary>
    /// Starts a scheduled event in this guild.
    /// </summary>
    /// <param name="guildEvent">The event to cancel.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Task StartEventAsync(DiscordScheduledGuildEvent guildEvent) => guildEvent.Status is not DiscordScheduledGuildEventStatus.Scheduled
            ? throw new InvalidOperationException("The event must be scheduled for it to be started.")
            : ModifyEventAsync(guildEvent, m => m.Status = DiscordScheduledGuildEventStatus.Active);

    /// <summary>
    /// Cancels an event. The event must be scheduled for it to be cancelled.
    /// </summary>
    /// <param name="guildEvent">The event to delete.</param>
    public Task CancelEventAsync(DiscordScheduledGuildEvent guildEvent) => guildEvent.Status is not DiscordScheduledGuildEventStatus.Scheduled
            ? throw new InvalidOperationException("The event must be scheduled for it to be cancelled.")
            : ModifyEventAsync(guildEvent, m => m.Status = DiscordScheduledGuildEventStatus.Cancelled);

    /// <summary>
    /// Modifies an existing scheduled event in this guild.
    /// </summary>
    /// <param name="guildEvent">The event to modify.</param>
    /// <param name="mdl">The action to perform on this event</param>
    /// <param name="reason">The reason this event is being modified</param>
    /// <returns>The modified object</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task ModifyEventAsync(DiscordScheduledGuildEvent guildEvent, Action<ScheduledGuildEventEditModel> mdl, string? reason = null)
    {
        ScheduledGuildEventEditModel model = new();
        mdl(model);

        if (model.Type.HasValue && model.Type.Value is not DiscordScheduledGuildEventType.External)
        {
            if (!model.Channel.HasValue)
            {
                throw new ArgumentException("Channel must be supplied if the event is a stage instance or voice channel event.");
            }

            if (model.Type.Value is DiscordScheduledGuildEventType.StageInstance && model.Channel.Value.Type is not DiscordChannelType.Stage)
            {
                throw new ArgumentException("Channel must be a stage channel if the event is a stage instance event.");
            }

            if (model.Type.Value is DiscordScheduledGuildEventType.VoiceChannel && model.Channel.Value.Type is not DiscordChannelType.Voice)
            {
                throw new ArgumentException("Channel must be a voice channel if the event is a voice channel event.");
            }

            if (model.EndTime.HasValue && model.EndTime.Value < guildEvent.StartTime)
            {
                throw new ArgumentException("End time must be after the start time.");
            }
        }

        if (model.Type.HasValue && model.Type.Value is DiscordScheduledGuildEventType.External)
        {
            if (!model.EndTime.HasValue)
            {
                throw new ArgumentException("End must be supplied if the event is an external event.");
            }

            if (!model.Metadata.HasValue || string.IsNullOrEmpty(model.Metadata.Value.Location))
            {
                throw new ArgumentException("Location must be supplied if the event is an external event.");
            }

            if (model.Channel.HasValue && model.Channel.Value != null)
            {
                throw new ArgumentException("Channel must not be supplied if the event is an external event.");
            }
        }

        if (guildEvent.Status is DiscordScheduledGuildEventStatus.Completed)
        {
            throw new ArgumentException("The event must not be completed for it to be modified.");
        }

        if (guildEvent.Status is DiscordScheduledGuildEventStatus.Cancelled)
        {
            throw new ArgumentException("The event must not be cancelled for it to be modified.");
        }

        if (model.Status.HasValue)
        {
            switch (model.Status.Value)
            {
                case DiscordScheduledGuildEventStatus.Scheduled:
                    throw new ArgumentException("Status must not be set to scheduled.");
                case DiscordScheduledGuildEventStatus.Active when guildEvent.Status is not DiscordScheduledGuildEventStatus.Scheduled:
                    throw new ArgumentException("Event status must be scheduled to progress to active.");
                case DiscordScheduledGuildEventStatus.Completed when guildEvent.Status is not DiscordScheduledGuildEventStatus.Active:
                    throw new ArgumentException("Event status must be active to progress to completed.");
                case DiscordScheduledGuildEventStatus.Cancelled when guildEvent.Status is not DiscordScheduledGuildEventStatus.Scheduled:
                    throw new ArgumentException("Event status must be scheduled to progress to cancelled.");
            }
        }

        DiscordScheduledGuildEvent modifiedEvent = await this.Discord.ApiClient.ModifyScheduledGuildEventAsync
        (
            this.Id,
            guildEvent.Id,
            model.Name,
            model.Description,
            model.Channel.IfPresent(c => c?.Id),
            model.StartTime,
            model.EndTime,
            model.Type,
            model.PrivacyLevel,
            model.Metadata,
            model.Status,
            model.CoverImage,
            reason
        );

        this.scheduledEvents[modifiedEvent.Id] = modifiedEvent;
    }

    /// <summary>
    /// Deletes an exising scheduled event in this guild.
    /// </summary>
    /// <param name="guildEvent"></param>
    /// <param name="reason">The reason which should be used for the audit log</param>
    /// <returns></returns>
    public async Task DeleteEventAsync(DiscordScheduledGuildEvent guildEvent, string? reason = null)
    {
        this.scheduledEvents.TryRemove(guildEvent.Id, out _);
        await this.Discord.ApiClient.DeleteScheduledGuildEventAsync(this.Id, guildEvent.Id, reason);
    }

    /// <summary>
    /// Deletes an exising scheduled event in this guild.
    /// </summary>
    /// <param name="guildEventId">The Id of the event which should be deleted.</param>
    /// <param name="reason">The reason which should be used for the audit log</param>
    /// <returns></returns>
    public async Task DeleteEventAsync(ulong guildEventId, string? reason = null)
    {
        this.scheduledEvents.TryRemove(guildEventId, out _);
        await this.Discord.ApiClient.DeleteScheduledGuildEventAsync(this.Id, guildEventId, reason);
    }

    /// <summary>
    /// Gets the currently active or scheduled events in this guild.
    /// </summary>
    /// <param name="withUserCounts">Whether to include number of users subscribed to each event</param>
    /// <returns>The active and scheduled events on the server, if any.</returns>
    public async Task<IReadOnlyList<DiscordScheduledGuildEvent>> GetEventsAsync(bool withUserCounts = false)
    {
        IReadOnlyList<DiscordScheduledGuildEvent> events = await this.Discord.ApiClient.GetScheduledGuildEventsAsync(this.Id, withUserCounts);

        foreach (DiscordScheduledGuildEvent @event in events)
        {
            this.scheduledEvents[@event.Id] = @event;
        }

        return events;
    }

    /// <summary>
    /// Gets a list of users who are interested in this event.
    /// </summary>
    /// <param name="guildEvent">The event to query users from</param>
    /// <param name="limit">How many users to fetch.</param>
    /// <param name="after">Fetch users after this id. Mutually exclusive with before</param>
    /// <param name="before">Fetch users before this id. Mutually exclusive with after</param>
    public IAsyncEnumerable<DiscordUser> GetEventUsersAsync
    (
        DiscordScheduledGuildEvent guildEvent,
        int limit = 100,
        ulong? after = null,
        ulong? before = null
    )
        => GetEventUsersAsync(guildEvent.Id, limit, after, before);

    /// <summary>
    /// Gets a list of users who are interested in this event.
    /// </summary>
    /// <param name="guildEventId">The id of the event to query users from</param>
    /// <param name="limit">How many users to fetch. The method performs one api call per 100 users</param>
    /// <param name="after">Fetch users after this id. Mutually exclusive with before</param>
    /// <param name="before">Fetch users before this id. Mutually exclusive with after</param>
    public async IAsyncEnumerable<DiscordUser> GetEventUsersAsync(ulong guildEventId, int limit = 100, ulong? after = null, ulong? before = null)
    {
        if (after.HasValue && before.HasValue)
        {
            throw new ArgumentException("after and before are mutually exclusive");
        }

        int remaining = limit;
        ulong? last = null;
        bool isBefore = before != null;
        int lastCount;
        do
        {
            int fetchSize = remaining > 100 ? 100 : remaining;
            IReadOnlyList<DiscordUser> fetch = await this.Discord.ApiClient.GetScheduledGuildEventUsersAsync(this.Id, guildEventId, true, fetchSize, isBefore ? last ?? before : null, !isBefore ? last ?? after : null);

            lastCount = fetch.Count;
            remaining -= lastCount;

            if (isBefore)
            {
                for (int i = lastCount - 1; i >= 0; i--)
                {
                    yield return fetch[i];
                }
                last = fetch.FirstOrDefault()?.Id;
            }
            else
            {
                for (int i = 0; i < lastCount; i++)
                {
                    yield return fetch[i];
                }
                last = fetch.LastOrDefault()?.Id;
            }
        }
        while (remaining > 0 && lastCount > 0);
    }

    /// <summary>
    /// Searches the current guild for members who's display name start with the specified name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="limit">The maximum amount of members to return. Max 1000. Defaults to 1.</param>
    /// <returns>The members found, if any.</returns>
    public async Task<IReadOnlyList<DiscordMember>> SearchMembersAsync(string name, int? limit = 1)
        => await this.Discord.ApiClient.SearchMembersAsync(this.Id, name, limit);

    /// <summary>
    /// Adds a new member to this guild
    /// </summary>
    /// <param name="user">User to add</param>
    /// <param name="accessToken">User's access token (OAuth2)</param>
    /// <param name="nickname">new nickname</param>
    /// <param name="muted">whether this user has to be muted</param>
    /// <param name="deaf">whether this user has to be deafened</param>
    /// <returns>Only returns the member if they were not already in the guild</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.CreateInvite" /> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the <paramref name="user"/> or <paramref name="accessToken"/> is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMember?> AddMemberAsync
    (
        DiscordUser user,
        string accessToken,
        string? nickname = null,
        bool muted = false,
        bool deaf = false
    )
        => await this.Discord.ApiClient.AddGuildMemberAsync(this.Id, user.Id, accessToken, muted, deaf, nickname, null);

    /// <summary>
    /// Adds a new member to this guild
    /// </summary>
    /// <param name="userId">The id of the User to add</param>
    /// <param name="accessToken">User's access token (OAuth2)</param>
    /// <param name="nickname">new nickname</param>
    /// <param name="muted">whether this user has to be muted</param>
    /// <param name="deaf">whether this user has to be deafened</param>
    /// <returns>Only returns the member if they were not already in the guild</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.CreateInvite" /> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the <paramref name="userId"/> or <paramref name="accessToken"/> is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMember?> AddMemberAsync
    (
        ulong userId,
        string accessToken,
        string? nickname = null,
        bool muted = false,
        bool deaf = false
    )
        => await this.Discord.ApiClient.AddGuildMemberAsync(this.Id, userId, accessToken, muted, deaf, nickname, null);

    /// <summary>
    /// Adds a new member to this guild
    /// </summary>
    /// <param name="user">User to add</param>
    /// <param name="accessToken">User's access token (OAuth2)</param>
    /// <param name="nickname">new nickname</param>
    /// <param name="roles">Ids of roles to add to the new member.</param>
    /// <param name="muted">whether this user has to be muted</param>
    /// <param name="deaf">whether this user has to be deafened</param>
    /// <returns>Only returns the member if they were not already in the guild</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.CreateInvite" /> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the <paramref name="user"/> or <paramref name="accessToken"/> is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMember?> AddMemberWithRolesAsync
    (
        DiscordUser user,
        string accessToken,
        IEnumerable<ulong> roles,
        string? nickname = null,
        bool muted = false,
        bool deaf = false
    )
        => await this.Discord.ApiClient.AddGuildMemberAsync(this.Id, user.Id, accessToken, muted, deaf, nickname, roles);

    /// <summary>
    /// Adds a new member to this guild
    /// </summary>
    /// <param name="userId">The id of the User to add</param>
    /// <param name="accessToken">User's access token (OAuth2)</param>
    /// <param name="nickname">new nickname</param>
    /// <param name="roles">Ids of roles to add to the new member.</param>
    /// <param name="muted">whether this user has to be muted</param>
    /// <param name="deaf">whether this user has to be deafened</param>
    /// <returns>Only returns the member if they were not already in the guild</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.CreateInvite" /> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the <paramref name="userId"/> or <paramref name="accessToken"/> is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMember?> AddMemberWithRolesAsync
    (
        ulong userId,
        string accessToken,
        IEnumerable<ulong> roles,
        string? nickname = null,
        bool muted = false,
        bool deaf = false
    )
        => await this.Discord.ApiClient.AddGuildMemberAsync(this.Id, userId, accessToken, muted, deaf, nickname, roles);

        /// <summary>
    /// Adds a new member to this guild
    /// </summary>
    /// <param name="user">User to add</param>
    /// <param name="accessToken">User's access token (OAuth2)</param>
    /// <param name="nickname">new nickname</param>
    /// <param name="roles">Collection of roles to add to the new member.</param>
    /// <param name="muted">whether this user has to be muted</param>
    /// <param name="deaf">whether this user has to be deafened</param>
    /// <returns>Only returns the member if they were not already in the guild</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.CreateInvite" /> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the <paramref name="user"/> or <paramref name="accessToken"/> is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMember?> AddMemberWithRolesAsync
    (
        DiscordUser user,
        string accessToken,
        IEnumerable<DiscordRole> roles,
        string? nickname = null,
        bool muted = false,
        bool deaf = false
    )
        => await this.Discord.ApiClient.AddGuildMemberAsync(this.Id, user.Id, accessToken, muted, deaf, nickname, roles?.Select(x => x.Id));

    /// <summary>
    /// Adds a new member to this guild
    /// </summary>
    /// <param name="userId">The id of the User to add</param>
    /// <param name="accessToken">User's access token (OAuth2)</param>
    /// <param name="nickname">new nickname</param>
    /// <param name="roles">Collection of roles to add to the new member.</param>
    /// <param name="muted">whether this user has to be muted</param>
    /// <param name="deaf">whether this user has to be deafened</param>
    /// <returns>Only returns the member if they were not already in the guild</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.CreateInvite" /> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the <paramref name="userId"/> or <paramref name="accessToken"/> is not found.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMember?> AddMemberWithRolesAsync
    (
        ulong userId,
        string accessToken,
        IEnumerable<DiscordRole> roles,
        string? nickname = null,
        bool muted = false,
        bool deaf = false
    )
        => await this.Discord.ApiClient.AddGuildMemberAsync(this.Id, userId, accessToken, muted, deaf, nickname, roles?.Select(x => x.Id));

    /// <summary>
    /// Deletes this guild. Requires the caller to be the owner of the guild.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client is not the owner of the guild.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteAsync()
        => await this.Discord.ApiClient.DeleteGuildAsync(this.Id);

    /// <summary>
    /// Modifies this guild.
    /// </summary>
    /// <param name="action">Action to perform on this guild..</param>
    /// <returns>The modified guild object.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuild> ModifyAsync(Action<GuildEditModel> action)
    {
        GuildEditModel mdl = new();
        action(mdl);

        if (mdl.AfkChannel.HasValue && mdl.AfkChannel.Value.Type != DiscordChannelType.Voice)
        {
            throw new ArgumentException("AFK channel needs to be a voice channel.");
        }

        Optional<string> iconb64 = Optional.FromNoValue<string>();

        if (mdl.Icon.HasValue && mdl.Icon.Value != null)
        {
            using InlineMediaTool imgtool = new(mdl.Icon.Value);
            iconb64 = imgtool.GetBase64();
        }
        else if (mdl.Icon.HasValue)
        {
            iconb64 = null;
        }

        Optional<string> splashb64 = Optional.FromNoValue<string>();

        if (mdl.Splash.HasValue && mdl.Splash.Value != null)
        {
            using InlineMediaTool imgtool = new(mdl.Splash.Value);
            splashb64 = imgtool.GetBase64();
        }
        else if (mdl.Splash.HasValue)
        {
            splashb64 = null;
        }

        Optional<string> bannerb64 = Optional.FromNoValue<string>();

        if (mdl.Banner.HasValue)
        {
            if (mdl.Banner.Value == null)
            {
                bannerb64 = null;
            }
            else
            {
                using InlineMediaTool imgtool = new(mdl.Banner.Value);
                bannerb64 = imgtool.GetBase64();
            }
        }

        return await this.Discord.ApiClient.ModifyGuildAsync(this.Id, mdl.Name, mdl.Region.IfPresent(e => e.Id),
            mdl.VerificationLevel, mdl.DefaultMessageNotifications, mdl.MfaLevel, mdl.ExplicitContentFilter,
            mdl.AfkChannel.IfPresent(e => e?.Id), mdl.AfkTimeout, iconb64, mdl.Owner.IfPresent(e => e.Id), splashb64,
            mdl.SystemChannel.IfPresent(e => e?.Id), bannerb64,
            mdl.Description, mdl.DiscoverySplash, mdl.Features, mdl.PreferredLocale,
            mdl.PublicUpdatesChannel.IfPresent(e => e?.Id), mdl.RulesChannel.IfPresent(e => e?.Id),
            mdl.SystemChannelFlags, mdl.AuditLogReason);
    }

    /// <summary>
    /// Gets the roles in this guild.
    /// </summary>
    /// <returns>All the roles in the guild.</returns>
    public async Task<IReadOnlyList<DiscordRole>> GetRolesAsync()
    {
        IReadOnlyList<DiscordRole> roles = await this.Discord.ApiClient.GetGuildRolesAsync(this.Id);
        this.roles = new ConcurrentDictionary<ulong, DiscordRole>(roles.ToDictionary(x => x.Id));
        return roles;
    }

    /// <summary>
    /// Gets a singular role from this guild by its ID.
    /// </summary>
    /// <param name="roleId">The ID of the role.</param>
    /// <param name="skipCache">Whether to skip checking cache for the role.</param>
    /// <returns>The role from the guild if it exists.</returns>
    public async Task<DiscordRole> GetRoleAsync(ulong roleId, bool skipCache = false)
    {
        if (!skipCache && this.roles.TryGetValue(roleId, out DiscordRole? role))
        {
            return role;
        }

        role = await this.Discord.ApiClient.GetGuildRoleAsync(this.Id, roleId);
        this.roles[role.Id] = role;
        return role;
    }

    /// <summary>
    /// Batch modifies the role order in the guild.
    /// </summary>
    /// <param name="roles">A dictionary of guild roles indexed by their new role positions.</param>
    /// <param name="reason">An optional Audit log reason on why this action was done.</param>
    /// <returns>A list of all the current guild roles ordered in their new role positions.</returns>
    public async Task<IReadOnlyList<DiscordRole>> ModifyRolePositionsAsync(IDictionary<int, DiscordRole> roles, string? reason = null)
    {
        if (roles.Count == 0)
        {
            throw new ArgumentException("Roles cannot be empty.", nameof(roles));
        }

        // Sort the roles by position and create skeleton roles for the payload.
        IReadOnlyList<DiscordRole> returnedRoles = await this.Discord.ApiClient.ModifyGuildRolePositionsAsync(this.Id, roles.Select(x => new RestGuildRoleReorderPayload() { RoleId = x.Value.Id, Position = x.Key }), reason);

        // Update the cache as the endpoint returns all roles in the order they were sent.
        this.roles = new(returnedRoles.Select(x => new KeyValuePair<ulong, DiscordRole>(x.Id, x)));
        return returnedRoles;
    }

    /// <summary>
    /// Removes a specified member from this guild.
    /// </summary>
    /// <param name="member">Member to remove.</param>
    /// <param name="reason">Reason for audit logs.</param>
    public async Task RemoveMemberAsync(DiscordUser member, string? reason = null)
        => await this.Discord.ApiClient.RemoveGuildMemberAsync(this.Id, member.Id, reason);

    /// <summary>
    /// Removes a specified member by ID.
    /// </summary>
    /// <param name="userId">ID of the user to remove.</param>
    /// <param name="reason">Reason for audit logs.</param>
    public async Task RemoveMemberAsync(ulong userId, string? reason = null)
        => await this.Discord.ApiClient.RemoveGuildMemberAsync(this.Id, userId, reason);

    /// <summary>
    /// Bans a specified member from this guild.
    /// </summary>
    /// <param name="member">Member to ban.</param>
    /// <param name="messageDeleteDuration">The duration in which discord should delete messages from the banned user.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.BanMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task BanMemberAsync(DiscordUser member, TimeSpan messageDeleteDuration = default, string? reason = null)
        => await this.Discord.ApiClient.CreateGuildBanAsync(this.Id, member.Id, (int)messageDeleteDuration.TotalSeconds, reason);

    /// <summary>
    /// Bans a specified user by ID. This doesn't require the user to be in this guild.
    /// </summary>
    /// <param name="userId">ID of the user to ban.</param>
    /// <param name="messageDeleteDuration">The duration in which discord should delete messages from the banned user.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.BanMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task BanMemberAsync(ulong userId, TimeSpan messageDeleteDuration = default, string? reason = null)
        => await this.Discord.ApiClient.CreateGuildBanAsync(this.Id, userId, (int)messageDeleteDuration.TotalSeconds, reason);

    /// <summary>
    /// Bans multiple users from this guild.
    /// </summary>
    /// <param name="users">Collection of users to ban</param>
    /// <param name="deleteMessageSeconds">Timespan in seconds to delete messages from the banned users</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>Response contains a which users were banned and which were not.</returns>
    public async Task<DiscordBulkBan> BulkBanMembersAsync(IEnumerable<DiscordUser> users, int deleteMessageSeconds = 0, string? reason = null)
    {
        IEnumerable<ulong> userIds = users.Select(x => x.Id);
        return await this.Discord.ApiClient.CreateGuildBulkBanAsync(this.Id, userIds, deleteMessageSeconds, reason);
    }

    /// <summary>
    /// Bans multiple users from this guild by their id
    /// </summary>
    /// <param name="userIds">Collection of user ids to ban</param>
    /// <param name="deleteMessageSeconds">Timespan in seconds to delete messages from the banned users</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>Response contains a which users were banned and which were not.</returns>
    public async Task<DiscordBulkBan> BulkBanMembersAsync(IEnumerable<ulong> userIds, int deleteMessageSeconds = 0, string? reason = null)
        => await this.Discord.ApiClient.CreateGuildBulkBanAsync(this.Id, userIds, deleteMessageSeconds, reason);

    /// <summary>
    /// Unbans a user from this guild.
    /// </summary>
    /// <param name="user">User to unban.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.BanMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task UnbanMemberAsync(DiscordUser user, string? reason = null)
        => await this.Discord.ApiClient.RemoveGuildBanAsync(this.Id, user.Id, reason);

    /// <summary>
    /// Unbans a user by ID.
    /// </summary>
    /// <param name="userId">ID of the user to unban.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.BanMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task UnbanMemberAsync(ulong userId, string? reason = null)
        => await this.Discord.ApiClient.RemoveGuildBanAsync(this.Id, userId, reason);

    /// <summary>
    /// Leaves this guild.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task LeaveAsync()
        => await this.Discord.ApiClient.LeaveGuildAsync(this.Id);

    /// <summary>
    /// Gets the bans for this guild.
    /// </summary>
    /// <param name="limit">The number of users to return (up to maximum 1000, default 1000).</param>
    /// <param name="before">Consider only users before the given user id.</param>
    /// <param name="after">Consider only users after the given user id.</param>
    /// <returns>Collection of bans in this guild.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.BanMembers"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordBan>> GetBansAsync(int? limit = null, ulong? before = null, ulong? after = null)
        => await this.Discord.ApiClient.GetGuildBansAsync(this.Id, limit, before, after);

    /// <summary>
    /// Gets a ban for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to get the ban for.</param>
    /// <exception cref="NotFoundException">Thrown when the specified user is not banned.</exception>
    /// <returns>The requested ban object.</returns>
    public async Task<DiscordBan> GetBanAsync(ulong userId)
        => await this.Discord.ApiClient.GetGuildBanAsync(this.Id, userId);

    /// <summary>
    /// Gets a ban for a specific user.
    /// </summary>
    /// <param name="user">The user to get the ban for.</param>
    /// <exception cref="NotFoundException">Thrown when the specified user is not banned.</exception>
    /// <returns>The requested ban object.</returns>
    public async Task<DiscordBan> GetBanAsync(DiscordUser user)
        => await this.Discord.ApiClient.GetGuildBanAsync(this.Id, user.Id);

    /// <summary>
    /// Creates a new text channel in this guild.
    /// </summary>
    /// <param name="name">Name of the new channel.</param>
    /// <param name="parent">Category to put this channel in.</param>
    /// <param name="topic">Topic of the channel.</param>
    /// <param name="overwrites">Permission overwrites for this channel.</param>
    /// <param name="nsfw">Whether the channel is to be flagged as not safe for work.</param>
    /// <param name="position">Sorting position of the channel.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
    /// <returns>The newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordChannel> CreateTextChannelAsync(string name, DiscordChannel? parent = null, Optional<string> topic = default, IEnumerable<DiscordOverwriteBuilder>? overwrites = null, bool? nsfw = null, Optional<int?> perUserRateLimit = default, int? position = null, string? reason = null)
        => CreateChannelAsync(name, DiscordChannelType.Text, parent, topic, null, null, overwrites, nsfw, perUserRateLimit, null, position, reason);

    /// <summary>
    /// Creates a new channel category in this guild.
    /// </summary>
    /// <param name="name">Name of the new category.</param>
    /// <param name="overwrites">Permission overwrites for this category.</param>
    /// <param name="position">Sorting position of the channel.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The newly-created channel category.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task<DiscordChannel> CreateChannelCategoryAsync(string name, IEnumerable<DiscordOverwriteBuilder>? overwrites = null, int? position = null, string? reason = null)
        => CreateChannelAsync(name, DiscordChannelType.Category, null, Optional.FromNoValue<string>(), null, null, overwrites, null, Optional.FromNoValue<int?>(), null, position, reason);

    /// <summary>
    /// Creates a new voice channel in this guild.
    /// </summary>
    /// <param name="name">Name of the new channel.</param>
    /// <param name="parent">Category to put this channel in.</param>
    /// <param name="bitrate">Bitrate of the channel.</param>
    /// <param name="userLimit">Maximum number of users in the channel.</param>
    /// <param name="overwrites">Permission overwrites for this channel.</param>
    /// <param name="qualityMode">Video quality mode of the channel.</param>
    /// <param name="position">Sorting position of the channel.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordChannel> CreateVoiceChannelAsync
    (
        string name,
        DiscordChannel? parent = null,
        int? bitrate = null,
        int? userLimit = null,
        IEnumerable<DiscordOverwriteBuilder>? overwrites = null,
        DiscordVideoQualityMode? qualityMode = null,
        int? position = null,
        string? reason = null
    ) => await CreateChannelAsync
        (
            name,
            DiscordChannelType.Voice,
            parent,
            Optional.FromNoValue<string>(),
            bitrate,
            userLimit,
            overwrites,
            null,
            Optional.FromNoValue<int?>(),
            qualityMode,
            position,
            reason
        );

    /// <summary>
    /// Creates a new channel in this guild.
    /// </summary>
    /// <param name="name">Name of the new channel.</param>
    /// <param name="type">Type of the new channel.</param>
    /// <param name="parent">Category to put this channel in.</param>
    /// <param name="topic">Topic of the channel.</param>
    /// <param name="bitrate">Bitrate of the channel. Applies to voice only.</param>
    /// <param name="userLimit">Maximum number of users in the channel. Applies to voice only.</param>
    /// <param name="overwrites">Permission overwrites for this channel.</param>
    /// <param name="nsfw">Whether the channel is to be flagged as not safe for work. Applies to text only.</param>
    /// <param name="perUserRateLimit">Slow mode timeout for users.</param>
    /// <param name="qualityMode">Video quality mode of the channel. Applies to voice only.</param>
    /// <param name="position">Sorting position of the channel.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <param name="defaultAutoArchiveDuration">The default duration in which threads (or posts) will archive.</param>
    /// <param name="defaultReactionEmoji">If applied to a forum, the default emoji to use for forum post reactions.</param>
    /// <param name="availableTags">The tags available for a post in this channel.</param>
    /// <param name="defaultSortOrder">The default sorting order.</param>
    /// <returns>The newly-created channel.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordChannel> CreateChannelAsync
    (
        string name,
        DiscordChannelType type,
        DiscordChannel? parent = null,
        Optional<string> topic = default,
        int? bitrate = null,
        int? userLimit = null,
        IEnumerable<DiscordOverwriteBuilder>? overwrites = null,
        bool? nsfw = null,
        Optional<int?> perUserRateLimit = default,
        DiscordVideoQualityMode? qualityMode = null,
        int? position = null,
        string? reason = null,
        DiscordAutoArchiveDuration? defaultAutoArchiveDuration = null,
        DefaultReaction? defaultReactionEmoji = null,
        IEnumerable<DiscordForumTagBuilder>? availableTags = null,
        DiscordDefaultSortOrder? defaultSortOrder = null
    ) =>
        // technically you can create news/store channels but not always
        type is not (DiscordChannelType.Text or DiscordChannelType.Voice or DiscordChannelType.Category or DiscordChannelType.News or DiscordChannelType.Stage or DiscordChannelType.GuildForum)
            ? throw new ArgumentException("Channel type must be text, voice, stage, category, or a forum.", nameof(type))
            : type == DiscordChannelType.Category && parent is not null
            ? throw new ArgumentException("Cannot specify parent of a channel category.", nameof(parent))
            : await this.Discord.ApiClient.CreateGuildChannelAsync
            (
                this.Id,
                name,
                type,
                parent?.Id,
                topic,
                bitrate,
                userLimit,
                overwrites,
                nsfw,
                perUserRateLimit,
                qualityMode,
                position,
                reason,
                defaultAutoArchiveDuration,
                defaultReactionEmoji,
                availableTags,
                defaultSortOrder
            );

    // this is to commemorate the Great DAPI Channel Massacre of 2017-11-19.
    /// <summary>
    /// <para>Deletes all channels in this guild.</para>
    /// <para>Note that this is irreversible. Use carefully!</para>
    /// </summary>
    /// <returns></returns>
    public Task DeleteAllChannelsAsync()
    {
        IEnumerable<Task> tasks = this.Channels.Values.Select(xc => xc.DeleteAsync());
        return Task.WhenAll(tasks);
    }

    /// <summary>
    /// Estimates the number of users to be pruned.
    /// </summary>
    /// <param name="days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
    /// <param name="includedRoles">The roles to be included in the prune.</param>
    /// <returns>Number of users that will be pruned.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.KickMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<int> GetPruneCountAsync(int days = 7, IEnumerable<DiscordRole> includedRoles = null)
    {
        if (includedRoles != null)
        {
            includedRoles = includedRoles.Where(r => r != null);
            int roleCount = includedRoles.Count();
            DiscordRole[] roleArr = includedRoles.ToArray();
            List<ulong> rawRoleIds = [];

            for (int i = 0; i < roleCount; i++)
            {
                if (this.roles.ContainsKey(roleArr[i].Id))
                {
                    rawRoleIds.Add(roleArr[i].Id);
                }
            }

            return await this.Discord.ApiClient.GetGuildPruneCountAsync(this.Id, days, rawRoleIds);
        }

        return await this.Discord.ApiClient.GetGuildPruneCountAsync(this.Id, days, null);
    }

    /// <summary>
    /// Estimates the number of users to be pruned.
    /// </summary>
    /// <param name="days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
    /// <param name="includedRoleIds">The ids of roles to be included in the prune.</param>
    /// <returns>Number of users that will be pruned.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.KickMembers"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<int> GetPruneCountAsync(int days = 7, IEnumerable<ulong>? includedRoleIds = null)
        => await this.Discord.ApiClient.GetGuildPruneCountAsync(this.Id, days, includedRoleIds?.Where(x => this.roles.ContainsKey(x)));

    /// <summary>
    /// Prunes inactive users from this guild.
    /// </summary>
    /// <param name="days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
    /// <param name="computePruneCount">Whether to return the prune count after this method completes. This is discouraged for larger guilds.</param>
    /// <param name="includedRoles">The roles to be included in the prune.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>Number of users pruned.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<int?> PruneAsync(int days = 7, bool computePruneCount = true, IEnumerable<DiscordRole>? includedRoles = null, string? reason = null)
    {
        if (includedRoles != null)
        {
            includedRoles = includedRoles.Where(r => r != null);
            int roleCount = includedRoles.Count();
            DiscordRole[] roleArr = includedRoles.ToArray();
            List<ulong> rawRoleIds = [];

            for (int i = 0; i < roleCount; i++)
            {
                if (this.roles.ContainsKey(roleArr[i].Id))
                {
                    rawRoleIds.Add(roleArr[i].Id);
                }
            }

            return await this.Discord.ApiClient.BeginGuildPruneAsync(this.Id, days, computePruneCount, rawRoleIds, reason);
        }

        return await this.Discord.ApiClient.BeginGuildPruneAsync(this.Id, days, computePruneCount, null, reason);
    }

    /// <summary>
    /// Prunes inactive users from this guild.
    /// </summary>
    /// <param name="days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
    /// <param name="computePruneCount">Whether to return the prune count after this method completes. This is discouraged for larger guilds.</param>
    /// <param name="includedRoleIds">The ids of roles to be included in the prune.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>Number of users pruned.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageChannels"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<int?> PruneAsync(int days = 7, bool computePruneCount = true, IEnumerable<ulong>? includedRoleIds = null, string? reason = null)
        => await this.Discord.ApiClient.BeginGuildPruneAsync(this.Id, days, computePruneCount, includedRoleIds?.Where(x => this.roles.ContainsKey(x)), reason);

    /// <summary>
    /// Gets integrations attached to this guild.
    /// </summary>
    /// <returns>Collection of integrations attached to this guild.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordIntegration>> GetIntegrationsAsync()
        => await this.Discord.ApiClient.GetGuildIntegrationsAsync(this.Id);

    /// <summary>
    /// Attaches an integration from current user to this guild.
    /// </summary>
    /// <param name="integration">Integration to attach.</param>
    /// <returns>The integration after being attached to the guild.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordIntegration> AttachUserIntegrationAsync(DiscordIntegration integration)
        => await this.Discord.ApiClient.CreateGuildIntegrationAsync(this.Id, integration.Type, integration.Id);

    /// <summary>
    /// Modifies an integration in this guild.
    /// </summary>
    /// <param name="integration">Integration to modify.</param>
    /// <param name="expireBehaviour">Number of days after which the integration expires.</param>
    /// <param name="expireGracePeriod">Length of grace period which allows for renewing the integration.</param>
    /// <param name="enableEmoticons">Whether emotes should be synced from this integration.</param>
    /// <returns>The modified integration.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordIntegration> ModifyIntegrationAsync(DiscordIntegration integration, int expireBehaviour, int expireGracePeriod, bool enableEmoticons)
        => await this.Discord.ApiClient.ModifyGuildIntegrationAsync(this.Id, integration.Id, expireBehaviour, expireGracePeriod, enableEmoticons);

    /// <summary>
    /// Modifies an integration in this guild.
    /// </summary>
    /// <param name="integrationId"> The id of the Integration to modify.</param>
    /// <param name="expireBehaviour">Number of days after which the integration expires.</param>
    /// <param name="expireGracePeriod">Length of grace period which allows for renewing the integration.</param>
    /// <param name="enableEmoticons">Whether emotes should be synced from this integration.</param>
    /// <returns>The modified integration.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordIntegration> ModifyIntegrationAsync(ulong integrationId, int expireBehaviour, int expireGracePeriod, bool enableEmoticons)
        => await this.Discord.ApiClient.ModifyGuildIntegrationAsync(this.Id, integrationId, expireBehaviour, expireGracePeriod, enableEmoticons);

    /// <summary>
    /// Removes an integration from this guild.
    /// </summary>
    /// <param name="integration">Integration to remove.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteIntegrationAsync(DiscordIntegration integration, string? reason = null)
        => await this.Discord.ApiClient.DeleteGuildIntegrationAsync(this.Id, integration.Id, reason);

    /// <summary>
    /// Removes an integration from this guild.
    /// </summary>
    /// <param name="integrationId">The id of the Integration to remove.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task DeleteIntegrationAsync(ulong integrationId, string? reason = null)
        => await this.Discord.ApiClient.DeleteGuildIntegrationAsync(this.Id, integrationId, reason);

    /// <summary>
    /// Forces re-synchronization of an integration for this guild.
    /// </summary>
    /// <param name="integration">Integration to synchronize.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task SyncIntegrationAsync(DiscordIntegration integration)
        => await this.Discord.ApiClient.SyncGuildIntegrationAsync(this.Id, integration.Id);

    /// <summary>
    /// Forces re-synchronization of an integration for this guild.
    /// </summary>
    /// <param name="integrationId">The id of the Integration to synchronize.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
    /// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task SyncIntegrationAsync(ulong integrationId)
        => await this.Discord.ApiClient.SyncGuildIntegrationAsync(this.Id, integrationId);

    /// <summary>
    /// Gets the voice regions for this guild.
    /// </summary>
    /// <returns>Voice regions available for this guild.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
    {
        IReadOnlyList<DiscordVoiceRegion> vrs = await this.Discord.ApiClient.GetGuildVoiceRegionsAsync(this.Id);
        foreach (DiscordVoiceRegion xvr in vrs)
        {
            this.Discord.InternalVoiceRegions.TryAdd(xvr.Id, xvr);
        }

        return vrs;
    }

    /// <summary>
    /// Gets the active and private threads for this guild.
    /// </summary>
    /// <returns>A list of all the active and private threads the user can access in the server.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<ThreadQueryResult> ListActiveThreadsAsync()
    {
        ThreadQueryResult threads = await this.Discord.ApiClient.ListActiveThreadsAsync(this.Id);
        // Gateway handles thread cache (if it does it properly
        /*foreach (var thread in threads)
            this.threads[thread.Id] = thread;*/
        return threads;
    }

    /// <summary>
    /// Gets an invite from this guild from an invite code.
    /// </summary>
    /// <param name="code">The invite code</param>
    /// <returns>An invite, or null if not in cache.</returns>
    public DiscordInvite GetInvite(string code)
        => this.invites.TryGetValue(code, out DiscordInvite? invite) ? invite : null;

    /// <summary>
    /// Gets all the invites created for all the channels in this guild.
    /// </summary>
    /// <returns>A collection of invites.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordInvite>> GetInvitesAsync()
    {
        IReadOnlyList<DiscordInvite> res = await this.Discord.ApiClient.GetGuildInvitesAsync(this.Id);

        DiscordIntents intents = this.Discord.Intents;

        if (!intents.HasIntent(DiscordIntents.GuildInvites))
        {
            for (int i = 0; i < res.Count; i++)
            {
                this.invites[res[i].Code] = res[i];
            }
        }

        return res;
    }

    /// <summary>
    /// Gets the vanity invite for this guild.
    /// </summary>
    /// <returns>A partial vanity invite.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordInvite> GetVanityInviteAsync()
        => await this.Discord.ApiClient.GetGuildVanityUrlAsync(this.Id);

    /// <summary>
    /// Gets all the webhooks created for all the channels in this guild.
    /// </summary>
    /// <returns>A collection of webhooks this guild has.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageWebhooks"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordWebhook>> GetWebhooksAsync()
        => await this.Discord.ApiClient.GetGuildWebhooksAsync(this.Id);

    /// <summary>
    /// Gets this guild's widget image.
    /// </summary>
    /// <param name="bannerType">The format of the widget.</param>
    /// <returns>The URL of the widget image.</returns>
    public string GetWidgetImage(DiscordWidgetType bannerType = DiscordWidgetType.Shield)
    {
        string param = bannerType switch
        {
            DiscordWidgetType.Banner1 => "banner1",
            DiscordWidgetType.Banner2 => "banner2",
            DiscordWidgetType.Banner3 => "banner3",
            DiscordWidgetType.Banner4 => "banner4",
            _ => "shield",
        };
        return $"{Endpoints.BASE_URI}/{Endpoints.GUILDS}/{this.Id}/{Endpoints.WIDGET_PNG}?style={param}";
    }

    /// <summary>
    /// Gets a member of this guild by their user ID.
    /// </summary>
    /// <param name="userId">ID of the member to get.</param>
    /// <param name="updateCache">Whether to always make a REST request and update the member cache.</param>
    /// <returns>The requested member.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <exception cref="NotFoundException">Thrown when the member does not exist in this guild.</exception>
    public async Task<DiscordMember> GetMemberAsync(ulong userId, bool updateCache = false)
    {
        if (!updateCache && this.members != null && this.members.TryGetValue(userId, out DiscordMember? mbr))
        {
            return mbr;
        }

        mbr = await this.Discord.ApiClient.GetGuildMemberAsync(this.Id, userId);

        DiscordIntents intents = this.Discord.Intents;

        if (intents.HasIntent(DiscordIntents.GuildMembers))
        {
            if (this.members != null)
            {
                this.members[userId] = mbr;
            }
        }

        return mbr;
    }

    /// <summary>
    /// Retrieves a full list of members from Discord. This method will bypass cache. This will execute one API request per 1000 entities.
    /// </summary>
    /// <param name="cancellationToken">Cancels the enumeration before the next api request</param>
    /// <returns>A collection of all members in this guild.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async IAsyncEnumerable<DiscordMember> GetAllMembersAsync
    (
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default
    )
    {
        int recievedLastCall = 1000;
        ulong last = 0ul;
        while (recievedLastCall == 1000)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            IReadOnlyList<TransportMember> transportMembers = await this.Discord.ApiClient.ListGuildMembersAsync(this.Id, 1000, last == 0 ? null : last);
            recievedLastCall = transportMembers.Count;

            foreach (TransportMember transportMember in transportMembers)
            {
                this.Discord.UpdateUserCache(new(transportMember.User)
                {
                    Discord = this.Discord
                });

                yield return new(transportMember)
                {
                    Discord = this.Discord,
                    guild_id = this.Id
                };
            }

            TransportMember? lastMember = transportMembers.LastOrDefault();
            last = lastMember?.User.Id ?? 0;
        }
    }

    /// <summary>
    /// Requests that Discord send a list of guild members based on the specified arguments. This method will fire the GuildMembersChunked event.
    /// <para>If no arguments aside from <paramref name="presences"/> and <paramref name="nonce"/> are specified, this will request all guild members.</para>
    /// </summary>
    /// <param name="query">Filters the returned members based on what the username starts with. Either this or <paramref name="userIds"/> must not be null.
    /// The <paramref name="limit"/> must also be greater than 0 if this is specified.</param>
    /// <param name="limit">Total number of members to request. This must be greater than 0 if <paramref name="query"/> is specified.</param>
    /// <param name="presences">Whether to include the <see cref="GuildMembersChunkedEventArgs.Presences"/> associated with the fetched members.</param>
    /// <param name="userIds">Whether to limit the request to the specified user ids. Either this or <paramref name="query"/> must not be null.</param>
    /// <param name="nonce">The unique string to identify the response. This must be unique per-guild if multiple requests to the same guild are made.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the iterator with.</param>
    /// <returns>An asynchronous iterator that will return all members.</returns>
    public async IAsyncEnumerable<DiscordMember> EnumerateRequestMembersAsync
    (
        string query = "",
        int limit = 0,
        bool? presences = null,
        IEnumerable<ulong>? userIds = null,
        string? nonce = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if (this.Discord is not DiscordClient client)
        {
            throw new InvalidOperationException("This operation is only valid for regular Discord clients.");
        }

        ChannelReader<GuildMembersChunkedEventArgs> reader = client.RegisterGuildMemberChunksEnumerator(this.Id, nonce);

        await RequestMembersAsync(query, limit, presences, userIds, nonce);

        await foreach (var evt in reader.ReadAllAsync(cancellationToken))
        {
            foreach (DiscordMember member in evt.Members)
            {
                yield return member;
            }
        }
    }

    /// <summary>
    /// Requests that Discord send a list of guild members based on the specified arguments. This method will fire the GuildMembersChunked event.
    /// <para>If no arguments aside from <paramref name="presences"/> and <paramref name="nonce"/> are specified, this will request all guild members.</para>
    /// </summary>
    /// <param name="query">Filters the returned members based on what the username starts with. Either this or <paramref name="userIds"/> must not be null.
    /// The <paramref name="limit"/> must also be greater than 0 if this is specified.</param>
    /// <param name="limit">Total number of members to request. This must be greater than 0 if <paramref name="query"/> is specified.</param>
    /// <param name="presences">Whether to include the <see cref="GuildMembersChunkedEventArgs.Presences"/> associated with the fetched members.</param>
    /// <param name="userIds">Whether to limit the request to the specified user ids. Either this or <paramref name="query"/> must not be null.</param>
    /// <param name="nonce">The unique string to identify the response.</param>
    public async Task RequestMembersAsync(string query = "", int limit = 0, bool? presences = null, IEnumerable<ulong>? userIds = null, string? nonce = null)
    {
        if (this.Discord is not DiscordClient client)
        {
            throw new InvalidOperationException("This operation is only valid for regular Discord clients.");
        }

        if (query == null && userIds == null)
        {
            throw new ArgumentException("The query and user IDs cannot both be null.");
        }

        if (query != null && userIds != null)
        {
            query = null;
        }

        GatewayRequestGuildMembers gatewayRequestGuildMembers = new(this)
        {
            Query = query,
            Limit = limit >= 0 ? limit : 0,
            Presences = presences,
            UserIds = userIds,
            Nonce = nonce
        };

#pragma warning disable DSP0004
        await client.SendPayloadAsync(GatewayOpCode.RequestGuildMembers, gatewayRequestGuildMembers, this.Id);
#pragma warning restore DSP0004
    }

    /// <summary>
    /// Gets all the channels this guild has.
    /// </summary>
    /// <returns>A collection of this guild's channels.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordChannel>> GetChannelsAsync()
        => await this.Discord.ApiClient.GetGuildChannelsAsync(this.Id);

    /// <summary>
    /// Creates a new role in this guild.
    /// </summary>
    /// <param name="name">Name of the role.</param>
    /// <param name="permissions">Permissions for the role.</param>
    /// <param name="color">Color for the role.</param>
    /// <param name="hoist">Whether the role is to be hoisted.</param>
    /// <param name="mentionable">Whether the role is to be mentionable.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <param name="icon">The icon to add to this role</param>
    /// <param name="emoji">The emoji to add to this role. Must be unicode.</param>
    /// <returns>The newly-created role.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageRoles"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordRole> CreateRoleAsync(string? name = null, DiscordPermissions? permissions = null, DiscordColor? color = null, bool? hoist = null, bool? mentionable = null, string? reason = null, Stream? icon = null, DiscordEmoji? emoji = null)
        => await this.Discord.ApiClient.CreateGuildRoleAsync(this.Id, name, permissions, color?.Value, hoist, mentionable, icon, emoji?.ToString(), reason);

    /// <summary>
    /// Gets a channel from this guild by its ID.
    /// </summary>
    /// <param name="id">ID of the channel to get.</param>
    /// <returns>Requested channel.</returns>
    internal DiscordChannel? GetChannel(ulong id)
        => this.channels != null && this.channels.TryGetValue(id, out DiscordChannel? channel) ? channel : null;

    /// <summary>
    /// Gets a channel from this guild by its ID.
    /// </summary>
    /// <param name="id">ID of the channel to get.</param>
    /// <param name="skipCache">If set to true this method will skip all caches and always perform a rest api call</param>
    /// <returns>Requested channel.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <exception cref="NotFoundException">Thrown when this channel does not exists</exception>
    /// <exception cref="InvalidOperationException">Thrown when the channel exists but does not belong to this guild instance.</exception>
    public async Task<DiscordChannel> GetChannelAsync(ulong id, bool skipCache = false)
    {
        DiscordChannel? channel;
        if (skipCache)
        {
            channel = await this.Discord.ApiClient.GetChannelAsync(id);

            if (channel.GuildId is null || (channel.GuildId is not null && channel.GuildId.Value != this.Id))
            {
                throw new InvalidOperationException("The channel exists but does not belong to this guild.");
            }

            return channel;
        }

        if (this.channels is not null && this.channels.TryGetValue(id, out channel))
        {
            return channel;
        }

        if (this.threads.TryGetValue(id, out DiscordThreadChannel? threadChannel))
        {
            return threadChannel;
        }

        channel = await this.Discord.ApiClient.GetChannelAsync(id);

        if (channel.GuildId is null || (channel.GuildId is not null && channel.GuildId.Value != this.Id))
        {
            throw new InvalidOperationException("The channel exists but does not belong to this guild.");
        }

        return channel;
    }

    /// <summary>
    /// Gets audit log entries for this guild.
    /// </summary>
    /// <param name="limit">Maximum number of entries to fetch. Defaults to 100</param>
    /// <param name="byMember">Filter by member responsible.</param>
    /// <param name="actionType">Filter by action type.</param>
    /// <returns>A collection of requested audit log entries.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ViewAuditLog"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <remarks>If you set <paramref name="limit"/> to null, it will fetch all entries. This may take a while as it will result in multiple api calls</remarks>
    public async IAsyncEnumerable<DiscordAuditLogEntry> GetAuditLogsAsync
    (
        int? limit = 100,
        DiscordMember? byMember = null,
        DiscordAuditLogActionType? actionType = null
    )
    {
        //Get all entries from api
        int entriesAcquiredLastCall = 1, totalEntriesCollected = 0, remainingEntries;
        ulong last = 0;
        while (entriesAcquiredLastCall > 0)
        {
            remainingEntries = limit != null ? limit.Value - totalEntriesCollected : 100;
            remainingEntries = Math.Min(100, remainingEntries);
            if (remainingEntries <= 0)
            {
                break;
            }

            AuditLog guildAuditLog = await this.Discord.ApiClient.GetAuditLogsAsync(this.Id, remainingEntries, null,
                last == 0 ? null : last, byMember?.Id, actionType);
            entriesAcquiredLastCall = guildAuditLog.Entries.Count();
            totalEntriesCollected += entriesAcquiredLastCall;
            if (entriesAcquiredLastCall > 0)
            {
                last = guildAuditLog.Entries.Last().Id;
                IAsyncEnumerable<DiscordAuditLogEntry> parsedEntries = AuditLogParser.ParseAuditLogToEntriesAsync(this, guildAuditLog);
                await foreach (DiscordAuditLogEntry discordAuditLogEntry in parsedEntries)
                {
                    yield return discordAuditLogEntry;
                }
            }

            if (limit.HasValue)
            {
                int remaining = limit.Value - totalEntriesCollected;
                if (remaining < 1)
                {
                    break;
                }
            }
            else if (entriesAcquiredLastCall < 100)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Gets all of this guild's custom emojis.
    /// </summary>
    /// <returns>All of this guild's custom emojis.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordGuildEmoji>> GetEmojisAsync()
        => await this.Discord.ApiClient.GetGuildEmojisAsync(this.Id);

    /// <summary>
    /// Gets this guild's specified custom emoji.
    /// </summary>
    /// <param name="id">ID of the emoji to get.</param>
    /// <returns>The requested custom emoji.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildEmoji> GetEmojiAsync(ulong id)
        => await this.Discord.ApiClient.GetGuildEmojiAsync(this.Id, id);

    /// <summary>
    /// Creates a new custom emoji for this guild.
    /// </summary>
    /// <param name="name">Name of the new emoji.</param>
    /// <param name="image">Image to use as the emoji.</param>
    /// <param name="roles">Roles for which the emoji will be available. This works only if your application is whitelisted as integration.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns>The newly-created emoji.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuildExpressions"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildEmoji> CreateEmojiAsync(string name, Stream image, IEnumerable<DiscordRole>? roles = null, string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        name = name.Trim();
        if (name.Length is < 2 or > 50)
        {
            throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.");
        }

        ArgumentNullException.ThrowIfNull(image);

        string? image64 = null;
        using (InlineMediaTool imgtool = new(image))
        {
            image64 = imgtool.GetBase64();
        }

        return await this.Discord.ApiClient.CreateGuildEmojiAsync(this.Id, name, image64, roles?.Select(xr => xr.Id), reason);
    }

    /// <summary>
    /// Modifies a this guild's custom emoji.
    /// </summary>
    /// <param name="emoji">Emoji to modify.</param>
    /// <param name="name">New name for the emoji.</param>
    /// <param name="roles">Roles for which the emoji will be available. This works only if your application is whitelisted as integration.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns>The modified emoji.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuildExpressions"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildEmoji> ModifyEmojiAsync(DiscordGuildEmoji emoji, string name, IEnumerable<DiscordRole>? roles = null, string? reason = null)
    {
        ArgumentNullException.ThrowIfNull(emoji);
        if (emoji.Guild.Id != this.Id)
        {
            throw new ArgumentException("This emoji does not belong to this guild.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        name = name.Trim();
        return name.Length is < 2 or > 50
            ? throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.")
            : await this.Discord.ApiClient.ModifyGuildEmojiAsync(this.Id, emoji.Id, name, roles?.Select(xr => xr.Id), reason);
    }

    /// <summary>
    /// Deletes this guild's custom emoji.
    /// </summary>
    /// <param name="emoji">Emoji to delete.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuildExpressions"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <exception cref="NotFoundException">Thrown when the emoji does not exist on this guild</exception>
    public async Task DeleteEmojiAsync(DiscordGuildEmoji emoji, string? reason = null)
    {
        ArgumentNullException.ThrowIfNull(emoji);

        if (emoji.Guild.Id != this.Id)
        {
            throw new ArgumentException("This emoji does not belong to this guild.");
        }
        else
        {
            await this.Discord.ApiClient.DeleteGuildEmojiAsync(this.Id, emoji.Id, reason);
        }
    }

    /// <summary>
    /// Deletes this guild's custom emoji.
    /// </summary>
    /// <param name="emojiId">Emoji to delete.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageGuildExpressions"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <exception cref="NotFoundException">Thrown when the emoji does not exist on this guild</exception>
    public async Task DeleteEmojiAsync(ulong emojiId, string? reason = null)
        => await this.Discord.ApiClient.DeleteGuildEmojiAsync(this.Id, emojiId, reason);

    /// <summary>
    /// <para>Gets the default channel for this guild.</para>
    /// <para>Default channel is the first channel current member can see.</para>
    /// </summary>
    /// <returns>This member's default guild.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public DiscordChannel? GetDefaultChannel()
    {
        return this.channels?.Values.Where(xc => xc.Type == DiscordChannelType.Text)
            .OrderBy(xc => xc.Position)
            .FirstOrDefault(xc => xc.PermissionsFor(this.CurrentMember).HasPermission(DiscordPermission.ViewChannel));
    }

    /// <summary>
    /// Gets the guild's widget
    /// </summary>
    /// <returns>The guild's widget</returns>
    public async Task<DiscordWidget> GetWidgetAsync()
        => await this.Discord.ApiClient.GetGuildWidgetAsync(this.Id);

    /// <summary>
    /// Gets the guild's widget settings
    /// </summary>
    /// <returns>The guild's widget settings</returns>
    public async Task<DiscordWidgetSettings> GetWidgetSettingsAsync()
        => await this.Discord.ApiClient.GetGuildWidgetSettingsAsync(this.Id);

    /// <summary>
    /// Modifies the guild's widget settings
    /// </summary>
    /// <param name="isEnabled">If the widget is enabled or not</param>
    /// <param name="channel">Widget channel</param>
    /// <param name="reason">Reason the widget settings were modified</param>
    /// <returns>The newly modified widget settings</returns>
    public async Task<DiscordWidgetSettings> ModifyWidgetSettingsAsync(bool? isEnabled = null, DiscordChannel? channel = null, string? reason = null)
        => await this.Discord.ApiClient.ModifyGuildWidgetSettingsAsync(this.Id, isEnabled, channel?.Id, reason);

    /// <summary>
    /// Gets all of this guild's templates.
    /// </summary>
    /// <returns>All of the guild's templates.</returns>
    /// <exception cref="UnauthorizedException">Throws when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<IReadOnlyList<DiscordGuildTemplate>> GetTemplatesAsync()
        => await this.Discord.ApiClient.GetGuildTemplatesAsync(this.Id);

    /// <summary>
    /// Creates a guild template.
    /// </summary>
    /// <param name="name">Name of the template.</param>
    /// <param name="description">Description of the template.</param>
    /// <returns>The template created.</returns>
    /// <exception cref="BadRequestException">Throws when a template already exists for the guild or a null parameter is provided for the name.</exception>
    /// <exception cref="UnauthorizedException">Throws when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildTemplate> CreateTemplateAsync(string name, string? description = null)
        => await this.Discord.ApiClient.CreateGuildTemplateAsync(this.Id, name, description);

    /// <summary>
    /// Syncs the template to the current guild's state.
    /// </summary>
    /// <param name="code">The code of the template to sync.</param>
    /// <returns>The template synced.</returns>
    /// <exception cref="NotFoundException">Throws when the template for the code cannot be found</exception>
    /// <exception cref="UnauthorizedException">Throws when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildTemplate> SyncTemplateAsync(string code)
        => await this.Discord.ApiClient.SyncGuildTemplateAsync(this.Id, code);

    /// <summary>
    /// Modifies the template's metadata.
    /// </summary>
    /// <param name="code">The template's code.</param>
    /// <param name="name">Name of the template.</param>
    /// <param name="description">Description of the template.</param>
    /// <returns>The template modified.</returns>
    /// <exception cref="NotFoundException">Throws when the template for the code cannot be found</exception>
    /// <exception cref="UnauthorizedException">Throws when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildTemplate> ModifyTemplateAsync(string code, string? name = null, string? description = null)
        => await this.Discord.ApiClient.ModifyGuildTemplateAsync(this.Id, code, name, description);

    /// <summary>
    /// Deletes the template.
    /// </summary>
    /// <param name="code">The code of the template to delete.</param>
    /// <returns>The deleted template.</returns>
    /// <exception cref="NotFoundException">Throws when the template for the code cannot be found</exception>
    /// <exception cref="UnauthorizedException">Throws when the client does not have the <see cref="DiscordPermission.ManageGuild"/> permission.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildTemplate> DeleteTemplateAsync(string code)
        => await this.Discord.ApiClient.DeleteGuildTemplateAsync(this.Id, code);

    /// <summary>
    /// Gets this guild's membership screening form.
    /// </summary>
    /// <returns>This guild's membership screening form.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildMembershipScreening> GetMembershipScreeningFormAsync()
        => await this.Discord.ApiClient.GetGuildMembershipScreeningFormAsync(this.Id);

    /// <summary>
    /// Modifies this guild's membership screening form.
    /// </summary>
    /// <param name="action">Action to perform</param>
    /// <returns>The modified screening form.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client doesn't have the <see cref="DiscordPermission.ManageGuild"/> permission, or community is not enabled on this guild.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildMembershipScreening> ModifyMembershipScreeningFormAsync(Action<MembershipScreeningEditModel> action)
    {
        MembershipScreeningEditModel editModel = new();
        action(editModel);
        return await this.Discord.ApiClient.ModifyGuildMembershipScreeningFormAsync(this.Id, editModel.Enabled, editModel.Fields, editModel.Description);
    }

    /// <summary>
    /// Gets a list of stickers from this guild.
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<DiscordMessageSticker>> GetStickersAsync()
        => await this.Discord.ApiClient.GetGuildStickersAsync(this.Id);

    /// <summary>
    /// Gets a sticker from this guild.
    /// </summary>
    /// <param name="stickerId">The id of the sticker.</param>
    /// <returns></returns>
    public async Task<DiscordMessageSticker> GetStickerAsync(ulong stickerId)
        => await this.Discord.ApiClient.GetGuildStickerAsync(this.Id, stickerId);

    /// <summary>
    /// Creates a sticker in this guild. Lottie stickers can only be created on verified and/or partnered servers.
    /// </summary>
    /// <param name="name">The name of the sticker.</param>
    /// <param name="description">The description of the sticker.</param>
    /// <param name="tags">The tags of the sticker. This must be a unicode emoji.</param>
    /// <param name="imageContents">The image content of the sticker.</param>
    /// <param name="format">The image format of the sticker.</param>
    /// <param name="reason">The reason this sticker is being created.</param>

    public async Task<DiscordMessageSticker> CreateStickerAsync(string name, string description, string tags, Stream imageContents, DiscordStickerFormat format, string? reason = null)
    {
        string contentType, extension;
        if (format is DiscordStickerFormat.PNG or DiscordStickerFormat.APNG)
        {
            contentType = "image/png";
            extension = "png";
        }
        else
        {
            if (!this.Features.Contains("PARTNERED") && !this.Features.Contains("VERIFIED"))
            {
                throw new InvalidOperationException("Lottie stickers can only be created on partnered or verified guilds.");
            }

            contentType = "application/json";
            extension = "json";
        }

        return await this.Discord.ApiClient.CreateGuildStickerAsync(this.Id, name, description ?? string.Empty, tags, new DiscordMessageFile(null, imageContents, null, extension, contentType), reason);
    }

    /// <summary>
    /// Modifies a sticker in this guild.
    /// </summary>
    /// <param name="stickerId">The id of the sticker.</param>
    /// <param name="action">Action to perform.</param>
    /// <param name="reason">Reason for audit log.</param>
    public async Task<DiscordMessageSticker> ModifyStickerAsync(ulong stickerId, Action<StickerEditModel> action, string? reason = null)
    {
        StickerEditModel editModel = new();
        action(editModel);
        return await this.Discord.ApiClient.ModifyStickerAsync(this.Id, stickerId, editModel.Name, editModel.Description, editModel.Tags, reason ?? editModel.AuditLogReason);
    }

    /// <summary>
    /// Modifies a sticker in this guild.
    /// </summary>
    /// <param name="sticker">Sticker to modify.</param>
    /// <param name="action">Action to perform.</param>
    /// <param name="reason">Reason for audit log.</param>
    public async Task<DiscordMessageSticker> ModifyStickerAsync(DiscordMessageSticker sticker, Action<StickerEditModel> action, string? reason = null)
    {
        StickerEditModel editModel = new();
        action(editModel);
        return await this.Discord.ApiClient.ModifyStickerAsync(this.Id, sticker.Id, editModel.Name, editModel.Description, editModel.Tags, reason ?? editModel.AuditLogReason);
    }

    /// <summary>
    /// Deletes a sticker in this guild.
    /// </summary>
    /// <param name="stickerId">The id of the sticker.</param>
    /// <param name="reason">Reason for audit log.</param>
    public async Task DeleteStickerAsync(ulong stickerId, string? reason = null)
        => await this.Discord.ApiClient.DeleteStickerAsync(this.Id, stickerId, reason);

    /// <summary>
    /// Deletes a sticker in this guild.
    /// </summary>
    /// <param name="sticker">Sticker to delete.</param>
    /// <param name="reason">Reason for audit log.</param>
    public async Task DeleteStickerAsync(DiscordMessageSticker sticker, string? reason = null)
        => await this.Discord.ApiClient.DeleteStickerAsync(this.Id, sticker.Id, reason);

    /// <summary>
    /// Gets all the application commands in this guild.
    /// </summary>
    /// <param name="withLocalizations">Whether to include localizations in the response.</param>
    /// <returns>A list of application commands in this guild.</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> GetApplicationCommandsAsync(bool withLocalizations = false) =>
        await this.Discord.ApiClient.GetGuildApplicationCommandsAsync(this.Discord.CurrentApplication.Id, this.Id, withLocalizations);

    /// <summary>
    /// Overwrites the existing application commands in this guild. New commands are automatically created and missing commands are automatically delete
    /// </summary>
    /// <param name="commands">The list of commands to overwrite with.</param>
    /// <returns>The list of guild commands</returns>
    public async Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteApplicationCommandsAsync(IEnumerable<DiscordApplicationCommand> commands) =>
        await this.Discord.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.Discord.CurrentApplication.Id, this.Id, commands);

    /// <summary>
    /// Creates or overwrites a application command in this guild.
    /// </summary>
    /// <param name="command">The command to create.</param>
    /// <returns>The created command.</returns>
    public async Task<DiscordApplicationCommand> CreateApplicationCommandAsync(DiscordApplicationCommand command) =>
        await this.Discord.ApiClient.CreateGuildApplicationCommandAsync(this.Discord.CurrentApplication.Id, this.Id, command);

    /// <summary>
    /// Edits a application command in this guild.
    /// </summary>
    /// <param name="commandId">The id of the command to edit.</param>
    /// <param name="action">Action to perform.</param>
    /// <returns>The edit command.</returns>
    public async Task<DiscordApplicationCommand> EditApplicationCommandAsync(ulong commandId, Action<ApplicationCommandEditModel> action)
    {
        ApplicationCommandEditModel editModel = new();
        action(editModel);
        return await this.Discord.ApiClient.EditGuildApplicationCommandAsync(this.Discord.CurrentApplication.Id, this.Id, commandId, editModel.Name, editModel.Description, editModel.Options, editModel.DefaultPermission, editModel.NSFW, default, default, editModel.AllowDMUsage, editModel.DefaultMemberPermissions);
    }

    /// <summary>
    /// Gets a application command in this guild by its id.
    /// </summary>
    /// <param name="commandId">The ID of the command to get.</param>
    /// <returns>The command with the ID.</returns>
    public async Task<DiscordApplicationCommand> GetApplicationCommandAsync(ulong commandId) =>
        await this.Discord.ApiClient.GetGlobalApplicationCommandAsync(this.Discord.CurrentApplication.Id, commandId);

    /// <summary>
    /// Gets a application command in this guild by its name.
    /// </summary>
    /// <param name="commandName">The name of the command to get.</param>
    /// <param name="withLocalizations">Whether to include localizations in the response.</param>
    /// <returns>The command with the name. This is null when the command is not found</returns>
    public async Task<DiscordApplicationCommand?> GetApplicationCommandAsync(string commandName, bool withLocalizations = false)
    {
        foreach (DiscordApplicationCommand command in await this.Discord.ApiClient.GetGlobalApplicationCommandsAsync(this.Discord.CurrentApplication.Id, withLocalizations))
        {
            if (command.Name == commandName)
            {
                return command;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets this guild's welcome screen.
    /// </summary>
    /// <returns>This guild's welcome screen object.</returns>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildWelcomeScreen> GetWelcomeScreenAsync() =>
        await this.Discord.ApiClient.GetGuildWelcomeScreenAsync(this.Id);

    /// <summary>
    /// Modifies this guild's welcome screen.
    /// </summary>
    /// <param name="action">Action to perform.</param>
    /// <param name="reason">Reason for audit log.</param>
    /// <returns>The modified welcome screen.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the client doesn't have the <see cref="DiscordPermission.ManageGuild"/> permission, or community is not enabled on this guild.</exception>
    /// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordGuildWelcomeScreen> ModifyWelcomeScreenAsync(Action<WelcomeScreenEditModel> action, string? reason = null)
    {
        WelcomeScreenEditModel editModel = new();
        action(editModel);
        return await this.Discord.ApiClient.ModifyGuildWelcomeScreenAsync(this.Id, editModel.Enabled, editModel.WelcomeChannels, editModel.Description, reason);
    }

    /// <summary>
    /// Gets all application command permissions in this guild.
    /// </summary>
    /// <returns>A list of permissions.</returns>
    public async Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> GetApplicationCommandsPermissionsAsync()
        => await this.Discord.ApiClient.GetGuildApplicationCommandPermissionsAsync(this.Discord.CurrentApplication.Id, this.Id);

    /// <summary>
    /// Gets permissions for a application command in this guild.
    /// </summary>
    /// <param name="command">The command to get them for.</param>
    /// <returns>The permissions.</returns>
    public async Task<DiscordGuildApplicationCommandPermissions> GetApplicationCommandPermissionsAsync(DiscordApplicationCommand command)
        => await this.Discord.ApiClient.GetApplicationCommandPermissionsAsync(this.Discord.CurrentApplication.Id, this.Id, command.Id);

    /// <summary>
    /// Edits permissions for a application command in this guild.
    /// </summary>
    /// <param name="command">The command to edit permissions for.</param>
    /// <param name="permissions">The list of permissions to use.</param>
    /// <returns>The edited permissions.</returns>
    public async Task<DiscordGuildApplicationCommandPermissions> EditApplicationCommandPermissionsAsync(DiscordApplicationCommand command, IEnumerable<DiscordApplicationCommandPermission> permissions)
        => await this.Discord.ApiClient.EditApplicationCommandPermissionsAsync(this.Discord.CurrentApplication.Id, this.Id, command.Id, permissions);

    /// <summary>
    /// Batch edits permissions for a application command in this guild.
    /// </summary>
    /// <param name="permissions">The list of permissions to use.</param>
    /// <returns>A list of edited permissions.</returns>
    public async Task<IReadOnlyList<DiscordGuildApplicationCommandPermissions>> BatchEditApplicationCommandPermissionsAsync(IEnumerable<DiscordGuildApplicationCommandPermissions> permissions)
        => await this.Discord.ApiClient.BatchEditApplicationCommandPermissionsAsync(this.Discord.CurrentApplication.Id, this.Id, permissions);

    /// <summary>
    /// Creates an auto-moderation rule in the guild.
    /// </summary>
    /// <param name="name">The rule name.</param>
    /// <param name="eventType">The event in which the rule should be triggered.</param>
    /// <param name="triggerType">The type of content which can trigger the rule.</param>
    /// <param name="triggerMetadata">Metadata used to determine whether a rule should be triggered. This argument can be skipped depending eventType value.</param>
    /// <param name="actions">Actions that will execute after the trigger of the rule.</param>
    /// <param name="enabled">Whether the rule is enabled or not.</param>
    /// <param name="exemptRoles">Roles that will not trigger the rule.</param>
    /// <param name="exemptChannels">Channels which will not trigger the rule.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns>The created rule.</returns>
    public async Task<DiscordAutoModerationRule> CreateAutoModerationRuleAsync
    (
        string name,
        DiscordRuleEventType eventType,
        DiscordRuleTriggerType triggerType,
        DiscordRuleTriggerMetadata triggerMetadata,
        IReadOnlyList<DiscordAutoModerationAction> actions,
        Optional<bool> enabled = default,
        Optional<IReadOnlyList<DiscordRole>> exemptRoles = default,
        Optional<IReadOnlyList<DiscordChannel>> exemptChannels = default,
        string? reason = null
    )
    {
        return await this.Discord.ApiClient.CreateGuildAutoModerationRuleAsync
        (
            this.Id,
            name,
            eventType,
            triggerType,
            triggerMetadata,
            actions,
            enabled,
            exemptRoles,
            exemptChannels,
            reason
        );
    }

    /// <summary>
    /// Gets an auto-moderation rule by an id.
    /// </summary>
    /// <param name="ruleId">The rule id.</param>
    /// <returns>The found rule.</returns>
    public async Task<DiscordAutoModerationRule> GetAutoModerationRuleAsync(ulong ruleId)
        => await this.Discord.ApiClient.GetGuildAutoModerationRuleAsync(this.Id, ruleId);

    /// <summary>
    /// Gets all auto-moderation rules in the guild.
    /// </summary>
    /// <returns>All rules available in the guild.</returns>
    public async Task<IReadOnlyList<DiscordAutoModerationRule>> GetAutoModerationRulesAsync()
        => await this.Discord.ApiClient.GetGuildAutoModerationRulesAsync(this.Id);

    /// <summary>
    /// Modify an auto-moderation rule in the guild.
    /// </summary>
    /// <param name="ruleId">The id of the rule that will be edited.</param>
    /// <param name="action">Action to perform on this rule.</param>
    /// <returns>The modified rule.</returns>
    /// <remarks>All arguments are optionals.</remarks>
    public async Task<DiscordAutoModerationRule> ModifyAutoModerationRuleAsync(ulong ruleId, Action<AutoModerationRuleEditModel> action)
    {
        AutoModerationRuleEditModel model = new();

        action(model);

        return await this.Discord.ApiClient.ModifyGuildAutoModerationRuleAsync
        (
            this.Id,
            ruleId,
            model.Name,
            model.EventType,
            model.TriggerMetadata,
            model.Actions,
            model.Enable,
            model.ExemptRoles,
            model.ExemptChannels,
            model.AuditLogReason
        );
    }

    /// <summary>
    /// Deletes a auto-moderation rule by an id.
    /// </summary>
    /// <param name="ruleId">The rule id.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    public async Task DeleteAutoModerationRuleAsync(ulong ruleId, string? reason = null)
        => await this.Discord.ApiClient.DeleteGuildAutoModerationRuleAsync(this.Id, ruleId, reason);

    #endregion

    /// <summary>
    /// Returns a string representation of this guild.
    /// </summary>
    /// <returns>String representation of this guild.</returns>
    public override string ToString() => $"Guild {this.Id}; {this.Name}";

    /// <summary>
    /// Checks whether this <see cref="DiscordGuild"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordGuild"/>.</returns>
    public override bool Equals(object obj) => Equals(obj as DiscordGuild);

    /// <summary>
    /// Checks whether this <see cref="DiscordGuild"/> is equal to another <see cref="DiscordGuild"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordGuild"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordGuild"/> is equal to this <see cref="DiscordGuild"/>.</returns>
    public bool Equals(DiscordGuild e) => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordGuild"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordGuild"/>.</returns>
    public override int GetHashCode() => this.Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="DiscordGuild"/> objects are equal.
    /// </summary>
    /// <param name="e1">First member to compare.</param>
    /// <param name="e2">Second member to compare.</param>
    /// <returns>Whether the two members are equal.</returns>
    public static bool operator ==(DiscordGuild e1, DiscordGuild e2)
    {
        object? o1 = e1;
        object? o2 = e2;

        return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordGuild"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First member to compare.</param>
    /// <param name="e2">Second member to compare.</param>
    /// <returns>Whether the two members are not equal.</returns>
    public static bool operator !=(DiscordGuild e1, DiscordGuild e2)
        => !(e1 == e2);
}

/// <summary>
/// Represents guild verification level.
/// </summary>
public enum DiscordVerificationLevel : int
{
    /// <summary>
    /// No verification. Anyone can join and chat right away.
    /// </summary>
    None = 0,

    /// <summary>
    /// Low verification level. Users are required to have a verified email attached to their account in order to be able to chat.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium verification level. Users are required to have a verified email attached to their account, and account age need to be at least 5 minutes in order to be able to chat.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// (╯°□°）╯︵ ┻━┻ verification level. Users are required to have a verified email attached to their account, account age need to be at least 5 minutes, and they need to be in the server for at least 10 minutes in order to be able to chat.
    /// </summary>
    High = 3,

    /// <summary>
    /// ┻━┻ ﾐヽ(ಠ益ಠ)ノ彡┻━┻ verification level. Users are required to have a verified phone number attached to their account.
    /// </summary>
    Highest = 4
}

/// <summary>
/// Represents default notification level for a guild.
/// </summary>
public enum DiscordDefaultMessageNotifications : int
{
    /// <summary>
    /// All messages will trigger push notifications.
    /// </summary>
    AllMessages = 0,

    /// <summary>
    /// Only messages that mention the user (or a role he's in) will trigger push notifications.
    /// </summary>
    MentionsOnly = 1
}

/// <summary>
/// Represents multi-factor authentication level required by a guild to use administrator functionality.
/// </summary>
public enum DiscordMfaLevel : int
{
    /// <summary>
    /// Multi-factor authentication is not required to use administrator functionality.
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// Multi-factor authentication is required to use administrator functionality.
    /// </summary>
    Enabled = 1
}

/// <summary>
/// Represents the value of explicit content filter in a guild.
/// </summary>
public enum DiscordExplicitContentFilter : int
{
    /// <summary>
    /// Explicit content filter is disabled.
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// Only messages from members without any roles are scanned.
    /// </summary>
    MembersWithoutRoles = 1,

    /// <summary>
    /// Messages from all members are scanned.
    /// </summary>
    AllMembers = 2
}

/// <summary>
/// Represents the formats for a guild widget.
/// </summary>
public enum DiscordWidgetType : int
{
    /// <summary>
    /// The widget is represented in shield format.
    /// <para>This is the default widget type.</para>
    /// </summary>
    Shield = 0,

    /// <summary>
    /// The widget is represented as the first banner type.
    /// </summary>
    Banner1 = 1,

    /// <summary>
    /// The widget is represented as the second banner type.
    /// </summary>
    Banner2 = 2,

    /// <summary>
    /// The widget is represented as the third banner type.
    /// </summary>
    Banner3 = 3,

    /// <summary>
    /// The widget is represented in the fourth banner type.
    /// </summary>
    Banner4 = 4
}
