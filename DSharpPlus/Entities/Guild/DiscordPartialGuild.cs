using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DSharpPlus.Net.Serialization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

public class DiscordPartialGuild
{
    /// <summary>
    /// Gets the guild´s id.
    /// </summary>
    [JsonProperty("id"), JsonRequired]
    public ulong Id { get; internal set; }
    
    /// <summary>
    /// Gets the guild's name.
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; internal set; }

    /// <summary>
    /// Gets the guild icon's hash.
    /// </summary>
    [JsonProperty("icon")]
    public string? IconHash { get; internal set; }

    /// <summary>
    /// Gets the guild splash's hash.
    /// </summary>
    [JsonProperty("splash")]
    public string? SplashHash { get; internal set; }

    /// <summary>
    /// Gets the guild discovery splash's hash.
    /// </summary>
    [JsonProperty("discovery_splash")]
    public string? DiscoverySplashHash { get; internal set; }

    /// <summary>
    /// Gets the preferred locale of this guild.
    /// <para>This is used for server discovery and notices from Discord. Defaults to en-US.</para>
    /// </summary>
    [JsonProperty("preferred_locale")]
    public string? PreferredLocale { get; internal set; }

    /// <summary>
    /// Gets the ID of the guild's owner.
    /// </summary>
    [JsonProperty("owner_id")]
    public ulong? OwnerId { get; internal set; }

    /// <summary>
    /// Gets permissions for the user in the guild (does not include channel overrides)
    /// </summary>
    [JsonProperty("permissions")]
    public Permissions? Permissions { get; set; }
    

    /// <summary>
    /// Gets the guild's voice region ID.
    /// </summary>
    [JsonProperty("region")]
    public string? VoiceRegionId { get; set; }
    

    /// <summary>
    /// Gets the guild's AFK voice channel ID.
    /// </summary>
    [JsonProperty("afk_channel_id")]
    public ulong? AfkChannelId { get; set; }
    

    /// <summary>
    /// Gets the guild's AFK timeout.
    /// </summary>
    [JsonProperty("afk_timeout")]
    public int? AfkTimeout { get; internal set; }

    /// <summary>
    /// Gets the guild's verification level.
    /// </summary>
    [JsonProperty("verification_level")]
    public VerificationLevel? VerificationLevel { get; internal set; }

    /// <summary>
    /// Gets the guild's default notification settings.
    /// </summary>
    [JsonProperty("default_message_notifications")]
    public DefaultMessageNotifications? DefaultMessageNotifications { get; internal set; }

    /// <summary>
    /// Gets the guild's explicit content filter settings.
    /// </summary>
    [JsonProperty("explicit_content_filter")]
    public ExplicitContentFilter? ExplicitContentFilter { get; internal set; }

    /// <summary>
    /// Gets the guild's nsfw level.
    /// </summary>
    [JsonProperty("nsfw_level")]
    public NsfwLevel NsfwLevel { get; internal set; }

    /// <summary>
    /// Gets the guild´s channel id in which the systems post
    /// </summary>
    [JsonProperty("system_channel_id")]
    public ulong? SystemChannelId { get; set; }

    /// <summary>
    /// Gets the settings for this guild's system channel.
    /// </summary>
    [JsonProperty("system_channel_flags")]
    public SystemChannelFlags? SystemChannelFlags { get; internal set; }

    /// <summary>
    /// Gets whether this guild's widget is enabled.
    /// </summary>
    [JsonProperty("widget_enabled")]
    public bool? WidgetEnabled { get; internal set; }

    [JsonProperty("widget_channel_id")]
    public ulong? WidgetChannelId { get; set; }

    [JsonProperty("rules_channel_id")]
    public ulong? RulesChannelId { get; set; }
    

    [JsonProperty("public_updates_channel_id")]
    public ulong? PublicUpdatesChannelId { get; set; }
    

    /// <summary>
    /// Gets the application ID of this guild if it is bot created.
    /// </summary>
    [JsonProperty("application_id")]
    public ulong? ApplicationId { get; internal set; }

    [JsonProperty("guild_scheduled_events"), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    public ConcurrentDictionary<ulong, DiscordScheduledGuildEvent>? ScheduledEvents = new();

    [JsonProperty("roles"), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    public ConcurrentDictionary<ulong, DiscordRole>? Roles;

    [JsonProperty("stickers"), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    public ConcurrentDictionary<ulong, DiscordMessageSticker>? Stickers = new();

    [JsonProperty("emojis"), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    public ConcurrentDictionary<ulong, DiscordEmoji>? Emojis;

    /// <summary>
    /// Gets a collection of this guild's features.
    /// </summary>
    [JsonProperty("features")]
    public IReadOnlyList<string>? Features { get; internal set; }

    /// <summary>
    /// Gets the required multi-factor authentication level for this guild.
    /// </summary>
    [JsonProperty("mfa_level")]
    public MfaLevel? MfaLevel { get; internal set; }

    /// <summary>
    /// Gets this guild's join date.
    /// </summary>
    [JsonProperty("joined_at")]
    public DateTimeOffset? JoinedAt { get; internal set; }

    /// <summary>
    /// Gets whether this guild is considered to be a large guild.
    /// </summary>
    [JsonProperty("large")]
    public bool? IsLarge { get; internal set; }

    /// <summary>
    /// Gets whether this guild is unavailable.
    /// </summary>
    [JsonProperty("unavailable")]
    public bool? IsUnavailable { get; internal set; }

    /// <summary>
    /// Gets the total number of members in this guild.
    /// </summary>
    [JsonProperty("member_count")]
    public int? MemberCount { get; internal set; }

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


    /// <summary>
    /// Gets the approximate number of members in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name="withCounts"></paramref> set to true.
    /// </summary>
    [JsonProperty("approximate_member_count")]
    public int? ApproximateMemberCount { get; internal set; }

    /// <summary>
    /// Gets the approximate number of presences in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name="withCounts"></paramref> set to true.
    /// </summary>
    [JsonProperty("approximate_presence_count")]
    public int? ApproximatePresenceCount { get; internal set; }


    /// <summary>
    /// Gets the maximum amount of users allowed per video channel.
    /// </summary>
    [JsonProperty("max_video_channel_users")]
    public int? MaxVideoChannelUsers { get; internal set; }

    [JsonProperty("voice_states"), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    public ConcurrentDictionary<ulong, DiscordVoiceState>? VoiceStates;

    [JsonProperty("members"), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    public ConcurrentDictionary<ulong, DiscordMember>? Members;

    [JsonProperty("channels"), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    public ConcurrentDictionary<ulong, DiscordChannel>? Channels;
    
    [JsonProperty("threads"), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    public ConcurrentDictionary<ulong, DiscordThreadChannel>? Threads = new();

    /// <summary>
    /// Gets whether the current user is the guild's owner.
    /// </summary>
    [JsonProperty("owner")]
    public bool? IsOwner;

    /// <summary>
    /// Gets the vanity URL code for this guild, when applicable.
    /// </summary>
    [JsonProperty("vanity_url_code")]
    public string? VanityUrlCode { get; internal set; }

    /// <summary>
    /// Gets the guild description, when applicable.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets this guild's banner hash, when applicable.
    /// </summary>
    [JsonProperty("banner")]
    public string? Banner { get; internal set; }

    /// <summary>
    /// Gets this guild's premium tier (Nitro boosting).
    /// </summary>
    [JsonProperty("premium_tier")]
    public PremiumTier? PremiumTier { get; internal set; }

    /// <summary>
    /// Gets the amount of members that boosted this guild.
    /// </summary>
    [JsonProperty("premium_subscription_count")]
    public int? PremiumSubscriptionCount { get; internal set; }

    /// <summary>
    /// Whether the guild has the boost progress bar enabled.
    /// </summary>
    [JsonProperty("premium_progress_bar_enabled")]
    public bool? PremiumProgressBarEnabled { get; internal set; }

    /// <summary>
    /// Gets whether this guild is designated as NSFW.
    /// </summary>
    [JsonProperty("nsfw")]
    public bool? IsNsfw { get; internal set; }

    [JsonProperty("stage_instances"), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
    public ConcurrentDictionary<ulong, DiscordStageInstance>? StageInstances;

    internal DiscordPartialGuild()
    {
    }
}
