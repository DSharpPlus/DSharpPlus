// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using DSharpPlus.Core.Enums;
using DSharpPlus.Core.GatewayPayloads;
using Newtonsoft.Json;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Guilds in Discord represent an isolated collection of users and channels, and are often referred to as "servers" in the UI.
    /// </summary>
    public sealed record DiscordGuild
    {
        /// <summary>
        /// The guild id.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The guild name (2-100 characters, excluding trailing and leading whitespace).
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; } = null!;

        /// <summary>
        /// The icon hash.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string? Icon { get; internal set; }

        /// <summary>
        /// The icon hash, returned when in the template object.
        /// </summary>
        [JsonProperty("icon_hash", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> IconHash { get; internal set; }

        /// <summary>
        /// The splash hash.
        /// </summary>
        [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
        public string? Splash { get; internal set; }

        /// <summary>
        /// The discovery splash hash; only present for guilds with the "DISCOVERABLE" feature.
        /// </summary>
        [JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
        public string? DiscoverySplash { get; internal set; }

        /// <summary>
        /// True if the user is the owner of the guild.
        /// </summary>
        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Owner { get; init; }

        /// <summary>
        /// The id of owner.
        /// </summary>
        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake OwnerId { get; init; } = null!;

        /// <summary>
        /// The total permissions for the user in the guild (excludes overwrites).
        /// </summary>
        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordPermissions> Permissions { get; internal set; }

        /// <summary>
        /// The voice region id for the guild (deprecated).
        /// </summary>
        [Obsolete($"This field is deprecated and is replaced by {nameof(DiscordChannel.RtcRegion)}")]
        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string?> Region { get; set; } = null!;

        /// <summary>
        /// The id of afk channel.
        /// </summary>
        [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? AfkChannelId { get; internal set; }

        /// <summary>
        /// The afk timeout in seconds.
        /// </summary>
        [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
        public int AfkTimeout { get; internal set; }

        /// <summary>
        /// True if the server widget is enabled.
        /// </summary>
        [JsonProperty("widget_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> WidgetEnabled { get; internal set; }

        /// <summary>
        /// The channel id that the widget will generate an invite to, or null if set to no invite.
        /// </summary>
        [JsonProperty("widget_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake?> WidgetChannelId { get; internal set; }

        /// <summary>
        /// The verification level required for the guild.
        /// </summary>
        [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildVerificationLevel VerificationLevel { get; internal set; }

        /// <summary>
        /// The default message notifications level.
        /// </summary>
        [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildMessageNotificationLevel DefaultMessageNotifications { get; internal set; }

        /// <summary>
        /// The explicit content filter level.
        /// </summary>
        [JsonProperty("explicit_content_filter", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildExplicitContentFilterLevel ExplicitContentFilter { get; internal set; }

        /// <summary>
        /// The roles in the guild.
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordRole[] Roles { get; internal set; } = null!;

        /// <summary>
        /// The custom guild emojis.
        /// </summary>
        [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordEmoji[] Emojis { get; internal set; } = null!;

        /// <summary>
        /// The enabled guild features.
        /// </summary>
        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Features { get; internal set; } = null!;

        /// <summary>
        /// The required MFA level for the guild.
        /// </summary>
        [JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildMFALevel MfaLevel { get; internal set; }

        /// <summary>
        /// The application id of the guild creator if it is bot-created.
        /// </summary>
        [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? ApplicationId { get; internal set; }

        /// <summary>
        /// The id of the channel where guild notices such as welcome messages and boost events are posted.
        /// </summary>
        [JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? SystemChannelId { get; internal set; }

        /// <summary>
        /// The system channel flags.
        /// </summary>
        [JsonProperty("system_channel_flags", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildSystemChannelFlags SystemChannelFlags { get; internal set; }

        /// <summary>
        /// The the id of the channel where Community guilds can display rules and/or guidelines.
        /// </summary>
        [JsonProperty("rules_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? RulesChannelId { get; internal set; }

        /// <summary>
        /// When this guild was joined at.
        /// </summary>
        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DateTimeOffset> JoinedAt { get; init; }

        /// <summary>
        /// True if this is considered a large guild.
        /// </summary>
        [JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Large { get; init; }

        /// <summary>
        /// True if this guild is unavailable due to an outage.
        /// </summary>
        [JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> Unavailable { get; init; }

        /// <summary>
        /// The total number of members in this guild.
        /// </summary>
        [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MemberCount { get; init; }

        /// <summary>
        /// The states of members currently in voice channels; lacks the guild_id key.
        /// </summary>
        [JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordVoiceState[]> VoiceStates { get; internal set; }

        /// <summary>
        /// The users in the guild.
        /// </summary>
        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordGuildMember[]> Members { get; internal set; }

        /// <summary>
        /// The channels in the guild.
        /// </summary>
        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordChannel[]> Channels { get; internal set; }

        /// <summary>
        /// All active threads in the guild that current user has permission to view.
        /// </summary>
        [JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordChannel[]> Threads { get; internal set; }

        /// <summary>
        /// The presences of the members in the guild, will only include non-offline members if the size is greater than large threshold.
        /// </summary>
        [JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordPresenceUpdatePayload[]> Presences { get; internal set; }

        /// <summary>
        /// The maximum number of presences for the guild (null is always returned, apart from the largest of guilds).
        /// </summary>
        [JsonProperty("max_presences", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int?> MaxPresences { get; internal set; }

        /// <summary>
        /// The maximum number of members for the guild.
        /// </summary>
        [JsonProperty("max_members", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MaxMembers { get; internal set; }

        /// <summary>
        /// The vanity url code for the guild.
        /// </summary>
        [JsonProperty("vanity_url_code", NullValueHandling = NullValueHandling.Ignore)]
        public string? VanityUrlCode { get; internal set; }

        /// <summary>
        /// The description of a Community guild.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; internal set; }

        /// <summary>
        /// The banner hash.
        /// </summary>
        [JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
        public string? Banner { get; internal set; }

        /// <summary>
        /// The premium tier (Server Boost level).
        /// </summary>
        [JsonProperty("premium_tier", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildPremiumTier PremiumTier { get; internal set; }

        /// <summary>
        /// The number of boosts this guild currently has.
        /// </summary>
        [JsonProperty("premium_subscription_count", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> PremiumSubscriptionCount { get; internal set; }

        /// <summary>
        /// The preferred locale of a Community guild; used in server discovery and notices from Discord, and sent in interactions; defaults to "en-US".
        /// </summary>
        [JsonProperty("preferred_locale", NullValueHandling = NullValueHandling.Ignore)]
        public string PreferredLocale { get; internal set; } = null!;

        /// <summary>
        /// The id of the channel where admins and moderators of Community guilds receive notices from Discord.
        /// </summary>
        [JsonProperty("public_updates_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? PublicUpdatesChannelId { get; internal set; }

        /// <summary>
        /// The maximum amount of users in a video channel.
        /// </summary>
        [JsonProperty("max_video_channel_users", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> MaxVideoChannelUsers { get; internal set; }

        /// <summary>
        /// The approximate number of members in this guild, returned from the <c>GET /guilds/:id</c> endpoint when <c>with_counts</c> is true.
        /// </summary>
        [JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> ApproximateMemberCount { get; internal set; }

        /// <summary>
        /// The approximate number of non-offline members in this guild, returned from the <c>GET /guilds/:id</c> endpoint when <c>with_counts</c> is true.
        /// </summary>
        [JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<int> ApproximatePresenceCount { get; internal set; }

        /// <summary>
        /// The welcome screen of a Community guild, shown to new members, returned in an Invite's guild object.
        /// </summary>
        [JsonProperty("welcome_screen", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordGuildWelcomeScreen> WelcomeScreen { get; internal set; }

        /// <summary>
        /// The guild NSFW level.
        /// </summary>
        [JsonProperty("nsfw_level", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordGuildNSFWLevel NSFWLevel { get; internal set; }

        /// <summary>
        /// Active stage instances in the guild.
        /// </summary>
        [JsonProperty("stage_instances", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordStageInstance[]> StageInstances { get; internal set; }

        /// <summary>
        /// Custom guild stickers.
        /// </summary>
        [JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSticker[]> Stickers { get; internal set; }

        /// <summary>
        /// The scheduled events in the guild.
        /// </summary>
        [JsonProperty("guild_scheduled_events", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordGuildScheduledEvent[]> GuildScheduledEvents { get; internal set; }

        /// <summary>
        /// Whether the guild has the boost progress bar enabled.
        /// </summary>
        [JsonProperty("premium_progress_bar_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool PremiumProgressBarEnabled { get; internal set; }
    }
}
