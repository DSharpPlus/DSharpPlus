namespace DSharpPlus.Net.Abstractions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Entities;
using Newtonsoft.Json;
using Serialization;

internal sealed record TransportGuild
{
    /// <summary>
    /// Gets the ID of this object.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong Id { get; internal set; }
    
    /// <summary>
    /// Gets the guild's name.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    internal string Name { get; set; }

    /// <summary>
    /// Gets the guild icon's hash.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
    internal string IconHash { get; set; }

    /// <summary>
    /// Gets the guild splash's hash.
    /// </summary>
    [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
    internal string SplashHash { get; set; }

    /// <summary>
    /// Gets the guild discovery splash's hash.
    /// </summary>
    [JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
    internal string DiscoverySplashHash { get; set; }

    /// <summary>
    /// Gets the preferred locale of this guild.
    /// <para>This is used for server discovery and notices from Discord. Defaults to en-US.</para>
    /// </summary>
    [JsonProperty("preferred_locale", NullValueHandling = NullValueHandling.Ignore)]
    internal string PreferredLocale { get; set; }

    /// <summary>
    /// Gets the ID of the guild's owner.
    /// </summary>
    [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong OwnerId { get; set; }

    /// <summary>
    /// Gets permissions for the user in the guild (does not include channel overrides)
    /// </summary>
    [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
    internal Permissions? Permissions { get; set; }

    /// <summary>
    /// Gets the guild's voice region ID.
    /// </summary>
    [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
    internal string VoiceRegionId { get; set; }

    /// <summary>
    /// Gets the guild's AFK voice channel ID.
    /// </summary>
    [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong AfkChannelId { get; set; }

    /// <summary>
    /// Gets the guild's AFK timeout.
    /// </summary>
    [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
    internal int AfkTimeout { get; set; }

    /// <summary>
    /// Gets the guild's verification level.
    /// </summary>
    [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
    internal VerificationLevel VerificationLevel { get; set; }

    /// <summary>
    /// Gets the guild's default notification settings.
    /// </summary>
    [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
    internal DefaultMessageNotifications DefaultMessageNotifications { get; set; }

    /// <summary>
    /// Gets the guild's explicit content filter settings.
    /// </summary>
    [JsonProperty("explicit_content_filter")]
    internal ExplicitContentFilter ExplicitContentFilter { get; set; }

    /// <summary>
    /// Gets the guild's nsfw level.
    /// </summary>
    [JsonProperty("nsfw_level")]
    internal NsfwLevel NsfwLevel { get; set; }

    [JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Include)]
    internal ulong? SystemChannelId { get; set; }
    
    /// <summary>
    /// Gets the settings for this guild's system channel.
    /// </summary>
    [JsonProperty("system_channel_flags")]
    internal SystemChannelFlags SystemChannelFlags { get; set; }

    [JsonProperty("safety_alerts_channel_id")]
    internal ulong? SafetyAlertsChannelId { get; set; }

    /// <summary>
    /// Gets whether this guild's widget is enabled.
    /// </summary>
    [JsonProperty("widget_enabled", NullValueHandling = NullValueHandling.Ignore)]
    internal bool? WidgetEnabled { get; set; }

    [JsonProperty("widget_channel_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong? WidgetChannelId { get; set; }

    [JsonProperty("rules_channel_id")]
    internal ulong? RulesChannelId { get; set; }

    [JsonProperty("public_updates_channel_id")]
    internal ulong? PublicUpdatesChannelId { get; set; }

    /// <summary>
    /// Gets the application ID of this guild if it is bot created.
    /// </summary>
    [JsonProperty("application_id")]
    internal ulong? ApplicationId { get; set; }

    [JsonProperty("guild_scheduled_events")]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordScheduledGuildEvent> ScheduledEvents = new();

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordRole> Roles;

    [JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordMessageSticker> Stickers = new();

    [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordEmoji> Emojis;

    /// <summary>
    /// Gets a collection of this guild's features.
    /// </summary>
    [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
    internal IReadOnlyList<string> Features { get; set; }

    /// <summary>
    /// Gets the required multi-factor authentication level for this guild.
    /// </summary>
    [JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
    internal MfaLevel MfaLevel { get; set; }

    /// <summary>
    /// Gets this guild's join date.
    /// </summary>
    [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
    internal DateTimeOffset JoinedAt { get; set; }

    /// <summary>
    /// Gets whether this guild is considered to be a large guild.
    /// </summary>
    [JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)]
    internal bool IsLarge { get; set; }

    /// <summary>
    /// Gets whether this guild is unavailable.
    /// </summary>
    [JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)]
    internal bool IsUnavailable { get; set; }

    /// <summary>
    /// Gets the total number of members in this guild.
    /// </summary>
    [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
    internal int MemberCount { get; set; }

    /// <summary>
    /// Gets the maximum amount of members allowed for this guild.
    /// </summary>
    [JsonProperty("max_members")]
    internal int? MaxMembers { get; set; }

    /// <summary>
    /// Gets the maximum amount of presences allowed for this guild.
    /// </summary>
    [JsonProperty("max_presences")]
    internal int? MaxPresences { get; set; }
    
    /// <summary>
    /// Gets the approximate number of members in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name="withCounts"></paramref> set to true.
    /// </summary>
    [JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
    internal int? ApproximateMemberCount { get; set; }

    /// <summary>
    /// Gets the approximate number of presences in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name="withCounts"></paramref> set to true.
    /// </summary>
    [JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
    internal int? ApproximatePresenceCount { get; set; }

    /// <summary>
    /// Gets the maximum amount of users allowed per video channel.
    /// </summary>
    [JsonProperty("max_video_channel_users", NullValueHandling = NullValueHandling.Ignore)]
    internal int? MaxVideoChannelUsers { get; set; }

    [JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordVoiceState> VoiceStates = new();

    [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordMember> Members = new();

    [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordChannel> Channels = new();

    [JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordThreadChannel> Threads = new();

    /// <summary>
    /// Gets whether the current user is the guild's owner.
    /// </summary>
    [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
    internal bool IsOwner { get; set; }
    
    /// <summary>
    /// Gets the vanity URL code for this guild, when applicable.
    /// </summary>
    [JsonProperty("vanity_url_code")]
    internal string VanityUrlCode { get; set; }

    /// <summary>
    /// Gets the guild description, when applicable.
    /// </summary>
    [JsonProperty("description")]
    internal string Description { get; set; }

    /// <summary>
    /// Gets this guild's banner hash, when applicable.
    /// </summary>
    [JsonProperty("banner")]
    internal string Banner { get; set; }

    /// <summary>
    /// Gets this guild's banner in url form.
    /// </summary>
    [JsonIgnore]
    internal string BannerUrl
        => !string.IsNullOrWhiteSpace(this.Banner) ? $"https://cdn.discordapp.com/banners/{this.Id}/{this.Banner}" : null;

    /// <summary>
    /// Gets this guild's premium tier (Nitro boosting).
    /// </summary>
    [JsonProperty("premium_tier")]
    internal PremiumTier PremiumTier { get; set; }

    /// <summary>
    /// Gets the amount of members that boosted this guild.
    /// </summary>
    [JsonProperty("premium_subscription_count", NullValueHandling = NullValueHandling.Ignore)]
    internal int? PremiumSubscriptionCount { get; set; }

    /// <summary>
    /// Whether the guild has the boost progress bar enabled.
    /// </summary>
    [JsonProperty("premium_progress_bar_enabled", NullValueHandling = NullValueHandling.Ignore)]
    internal bool PremiumProgressBarEnabled { get; set; }

    /// <summary>
    /// Gets whether this guild is designated as NSFW.
    /// </summary>
    [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
    internal bool IsNsfw { get; set; }

    /// <summary>
    /// Gets the stage instances in this guild.
    /// </summary>
    [JsonIgnore]
    internal IReadOnlyDictionary<ulong, DiscordStageInstance> StageInstances => new ReadOnlyConcurrentDictionary<ulong, DiscordStageInstance>(this._stageInstances);

    [JsonProperty("stage_instances", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    internal ConcurrentDictionary<ulong, DiscordStageInstance> StageInstances;

    // Failed attempts so far: 8
    // Velvet got it working in one attempt. I'm not mad, why would I be mad. - Lunar
    /// <summary>
    /// Gets channels ordered in a manner in which they'd be ordered in the UI of the discord client.
    /// </summary>
    [JsonIgnore]
    // Group the channels by category or parent id
    internal IEnumerable<DiscordChannel> OrderedChannels => _channels.Values.GroupBy(channel => channel.IsCategory ? channel.Id : channel.ParentId)
        // Order the channel by the category's position
        .OrderBy(channels => channels.FirstOrDefault(channel => channel.IsCategory)?.Position)
        // Select the category's channels
        // Order them by text, shoving voice or stage types to the bottom
        // Then order them by their position
        .Select(channel => channel.OrderBy(channel => channel.Type is ChannelType.Voice or ChannelType.Stage).ThenBy(channel => channel.Position))
        // Group them all back together into a single enumerable.
        .SelectMany(channel => channel);

    [JsonIgnore]
    internal bool IsSynced { get; set; }

    internal DiscordGuild()
    {
        this._currentMemberLazy = new Lazy<DiscordMember>(() => this._members != null && this._members.TryGetValue(this.Discord.CurrentUser.Id, out DiscordMember? member) ? member : null);
        this.Invites = new ConcurrentDictionary<string, DiscordInvite>();
    }
}
