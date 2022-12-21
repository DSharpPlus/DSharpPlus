using System;
using System.Collections.Generic;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Guilds in Internal represent an isolated collection of users and channels, and are often referred to as "servers" in the UI.
    /// </summary>
    [InternalGatewayPayload("GUILD_CREATE", "GUILD_UPDATE", "GUILD_DELETE")]
    public sealed record InternalGuild
    {
        /// <summary>
        /// The guild id.
        /// </summary>
        [JsonPropertyName("id")]
        public InternalSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild name (2-100 characters, excluding trailing and leading whitespace).
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The icon hash.
        /// </summary>
        [JsonPropertyName("icon")]
        public string? Icon { get; init; }

        /// <summary>
        /// The icon hash, returned when in the template object.
        /// </summary>
        [JsonPropertyName("icon_hash")]
        public Optional<string?> IconHash { get; init; }

        /// <summary>
        /// The splash hash.
        /// </summary>
        [JsonPropertyName("splash")]
        public string? Splash { get; init; }

        /// <summary>
        /// The discovery splash hash; only present for guilds with the <see cref="InternalGuildFeature.Discoverable"/> feature.
        /// </summary>
        [JsonPropertyName("discovery_splash")]
        public string? DiscoverySplash { get; init; }

        /// <summary>
        /// True if the user is the owner of the guild.
        /// </summary>
        [JsonPropertyName("owner")]
        public Optional<bool> Owner { get; init; }

        /// <summary>
        /// The id of owner.
        /// </summary>
        [JsonPropertyName("owner_id")]
        public InternalSnowflake OwnerId { get; init; } = null!;

        /// <summary>
        /// The total permissions for the user in the guild (excludes overwrites).
        /// </summary>
        [JsonPropertyName("permissions")]
        public Optional<InternalPermissions> Permissions { get; init; }

        /// <summary>
        /// The voice region id for the guild (deprecated).
        /// </summary>
        [Obsolete($"This field is deprecated and is replaced by {nameof(InternalChannel.RtcRegion)}")]
        [JsonPropertyName("region")]
        public Optional<string?> Region { get; set; } = null!;

        /// <summary>
        /// The id of afk channel.
        /// </summary>
        [JsonPropertyName("afk_channel_id")]
        public InternalSnowflake? AfkChannelId { get; init; }

        /// <summary>
        /// The afk timeout in seconds.
        /// </summary>
        [JsonPropertyName("afk_timeout")]
        public int AfkTimeout { get; init; }

        /// <summary>
        /// True if the server widget is enabled.
        /// </summary>
        [JsonPropertyName("widget_enabled")]
        public Optional<bool> WidgetEnabled { get; init; }

        /// <summary>
        /// The channel id that the widget will generate an invite to, or null if set to no invite.
        /// </summary>
        [JsonPropertyName("widget_channel_id")]
        public Optional<InternalSnowflake?> WidgetChannelId { get; init; }

        /// <summary>
        /// The verification level required for the guild.
        /// </summary>
        [JsonPropertyName("verification_level")]
        public InternalGuildVerificationLevel VerificationLevel { get; init; }

        /// <summary>
        /// The default message notifications level.
        /// </summary>
        [JsonPropertyName("default_message_notifications")]
        public InternalGuildMessageNotificationLevel DefaultMessageNotifications { get; init; }

        /// <summary>
        /// The explicit content filter level.
        /// </summary>
        [JsonPropertyName("explicit_content_filter")]
        public InternalGuildExplicitContentFilterLevel ExplicitContentFilter { get; init; }

        /// <summary>
        /// The roles in the guild.
        /// </summary>
        [JsonPropertyName("roles")]
        public IReadOnlyList<InternalRole> Roles { get; init; } = Array.Empty<InternalRole>();

        /// <summary>
        /// The custom guild emojis.
        /// </summary>
        [JsonPropertyName("emojis")]
        public IReadOnlyList<InternalEmoji> Emojis { get; init; } = Array.Empty<InternalEmoji>();

        /// <summary>
        /// The enabled guild features.
        /// </summary>
        /// <remarks>
        /// See <see cref="InternalGuildFeature"/> for more information.
        /// </remarks>
        [JsonPropertyName("features")]
        public IReadOnlyList<string> Features { get; init; } = Array.Empty<string>();

        /// <summary>
        /// The required MFA level for the guild.
        /// </summary>
        [JsonPropertyName("mfa_level")]
        public InternalGuildMFALevel MfaLevel { get; init; }

        /// <summary>
        /// The application id of the guild creator if it is bot-created.
        /// </summary>
        [JsonPropertyName("application_id")]
        public InternalSnowflake? ApplicationId { get; init; }

        /// <summary>
        /// The id of the channel where guild notices such as welcome messages and boost events are posted.
        /// </summary>
        [JsonPropertyName("system_channel_id")]
        public InternalSnowflake? SystemChannelId { get; init; }

        /// <summary>
        /// The system channel flags.
        /// </summary>
        [JsonPropertyName("system_channel_flags")]
        public InternalGuildSystemChannelFlags SystemChannelFlags { get; init; }

        /// <summary>
        /// The the id of the channel where Community guilds can display rules and/or guidelines.
        /// </summary>
        [JsonPropertyName("rules_channel_id")]
        public InternalSnowflake? RulesChannelId { get; init; }

        /// <summary>
        /// The maximum number of presences for the guild (null is always returned, apart from the largest of guilds).
        /// </summary>
        [JsonPropertyName("max_presences")]
        public Optional<int?> MaxPresences { get; init; }

        /// <summary>
        /// The maximum number of members for the guild.
        /// </summary>
        [JsonPropertyName("max_members")]
        public Optional<int> MaxMembers { get; init; }

        /// <summary>
        /// The vanity url code for the guild.
        /// </summary>
        [JsonPropertyName("vanity_url_code")]
        public string? VanityUrlCode { get; init; }

        /// <summary>
        /// The description of a Community guild.
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; init; }

        /// <summary>
        /// The banner hash.
        /// </summary>
        [JsonPropertyName("banner")]
        public string? Banner { get; init; }

        /// <summary>
        /// The premium tier (Server Boost level).
        /// </summary>
        [JsonPropertyName("premium_tier")]
        public InternalGuildPremiumTier PremiumTier { get; init; }

        /// <summary>
        /// The number of boosts this guild currently has.
        /// </summary>
        [JsonPropertyName("premium_subscription_count")]
        public Optional<int> PremiumSubscriptionCount { get; init; }

        /// <summary>
        /// The preferred locale of a Community guild; used in server discovery and notices from Internal, and sent in interactions; defaults to "en-US".
        /// </summary>
        [JsonPropertyName("preferred_locale")]
        public string PreferredLocale { get; init; } = null!;

        /// <summary>
        /// The id of the channel where admins and moderators of Community guilds receive notices from Internal.
        /// </summary>
        [JsonPropertyName("public_updates_channel_id")]
        public InternalSnowflake? PublicUpdatesChannelId { get; init; }

        /// <summary>
        /// The maximum amount of users in a video channel.
        /// </summary>
        [JsonPropertyName("max_video_channel_users")]
        public Optional<int> MaxVideoChannelUsers { get; init; }

        /// <summary>
        /// The approximate number of members in this guild, returned from the <c>GET /guilds/:id</c> endpoint when <c>with_counts</c> is true.
        /// </summary>
        [JsonPropertyName("approximate_member_count")]
        public Optional<int> ApproximateMemberCount { get; init; }

        /// <summary>
        /// The approximate number of non-offline members in this guild, returned from the <c>GET /guilds/:id</c> endpoint when <c>with_counts</c> is true.
        /// </summary>
        [JsonPropertyName("approximate_presence_count")]
        public Optional<int> ApproximatePresenceCount { get; init; }

        /// <summary>
        /// The welcome screen of a Community guild, shown to new members, returned in an Invite's guild object.
        /// </summary>
        [JsonPropertyName("welcome_screen")]
        public Optional<InternalGuildWelcomeScreen> WelcomeScreen { get; init; }

        /// <summary>
        /// The guild NSFW level.
        /// </summary>
        [JsonPropertyName("nsfw_level")]
        public InternalGuildNSFWLevel NSFWLevel { get; init; }

        /// <summary>
        /// Custom guild stickers.
        /// </summary>
        [JsonPropertyName("stickers")]
        public Optional<IReadOnlyList<InternalSticker>> Stickers { get; init; }

        /// <summary>
        /// Whether the guild has the boost progress bar enabled.
        /// </summary>
        [JsonPropertyName("premium_progress_bar_enabled")]
        public bool PremiumProgressBarEnabled { get; init; }
    }
}
