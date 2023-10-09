// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a partially populated guild object.
/// </summary>
public interface IPartialGuild
{
    /// <summary>
    /// The snowflake identifier of this guild.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The name of this guild, between 2 and 100 characters.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The icon hash for this guild.
    /// </summary>
    public Optional<string?> Icon { get; }

    /// <summary>
    /// The icon hash for this guild. This field, unlike <seealso cref="Icon"/>, is only sent when a part of
    /// the guild template object.
    /// </summary>
    public Optional<string?> IconHash { get; }

    /// <summary>
    /// The splash hash for this guild.
    /// </summary>
    public Optional<string?> Splash { get; }

    /// <summary>
    /// The discovery splash hash for this guild, only present for discoverable guilds.
    /// </summary>
    public Optional<string?> DiscoverySplash { get; }

    /// <summary>
    /// Indicates whether the current user owns this guild.
    /// </summary>
    public Optional<bool> Owner { get; }

    /// <summary>
    /// The snowflake identifier of the user owning this guild.
    /// </summary>
    public Optional<Snowflake> OwnerId { get; }

    /// <summary>
    /// The permissions for the current user, excluding overwrites.
    /// </summary>
    public Optional<DiscordPermissions> Permissions { get; }

    /// <summary>
    /// The snowflake identifier of the afk voice channel.
    /// </summary>
    public Optional<Snowflake?> AfkChannelId { get; }

    /// <summary>
    /// The voice afk timeout in seconds.
    /// </summary>
    public Optional<int> AfkTimeout { get; }

    /// <summary>
    /// Indicates whether the guild widget is enabled.
    /// </summary>
    public Optional<bool> WidgetEnabled { get; }

    /// <summary>
    /// The snowflake identifier of the channel that the guild widget will generate an invite to.
    /// </summary>
    public Optional<Snowflake?> WidgetChannelId { get; }

    /// <summary>
    /// Indicates the verification level required to chat in this guild.
    /// </summary>
    public Optional<DiscordVerificationLevel> VerificationLevel { get; }

    /// <summary>
    /// Indicates the default message notification setting.
    /// </summary>
    public Optional<DiscordMessageNotificationLevel> DefaultMessageNotifications { get; }

    /// <summary>
    /// Indicates the default severity level of the explicit content filter.
    /// </summary>
    public Optional<DiscordExplicitContentFilterLevel> ExplicitContentFilter { get; }

    /// <summary>
    /// The roles within this guild.
    /// </summary>
    public Optional<IReadOnlyList<IRole>> Roles { get; }

    /// <summary>
    /// The custom guild emojis for this guild.
    /// </summary>
    public Optional<IReadOnlyList<IEmoji>> Emojis { get; }

    /// <summary>
    /// The enabled guild feature identifiers for this guild.
    /// </summary>
    public Optional<IReadOnlyList<string>> Features { get; }

    /// <summary>
    /// The required MFA level for moderation actions in this guild.
    /// </summary>
    public Optional<DiscordMfaLevel> MfaLevel { get; }

    /// <summary>
    /// The snowflake identifier of the application that created this guild, if it was created by a bot.
    /// </summary>
    public Optional<Snowflake?> ApplicationId { get; }

    /// <summary>
    /// The snowflake identifier of the channel where guild notices such as welcome messages and boost
    /// messages are sent.
    /// </summary>
    public Optional<Snowflake?> SystemChannelId { get; }

    /// <summary>
    /// Additional settings for the system channel in this guild, represented as flags.
    /// </summary>
    public Optional<DiscordSystemChannelFlags> SystemChannelFlags { get; }

    /// <summary>
    /// The snowflake identifier of the channel where community servers display rules and guidelines.
    /// </summary>
    public Optional<Snowflake?> RulesChannelId { get; }

    /// <summary>
    /// The maximum number of presences for this guild. This will nearly always be <see langword="null"/>,
    /// except for the largest guilds.
    /// </summary>
    public Optional<int?> MaxPresences { get; }

    /// <summary>
    /// The member limit for this guild.
    /// </summary>
    public Optional<int> MaxMembers { get; }

    /// <summary>
    /// The vanity invite code for this guild.
    /// </summary>
    public Optional<string?> VanityUrlCode { get; }

    /// <summary>
    /// The description of this guild.
    /// </summary>
    public Optional<string?> Description { get; }

    /// <summary>
    /// The banner image hash for this guild.
    /// </summary>
    public Optional<string?> Banner { get; }

    /// <summary>
    /// The server boost level for this guild.
    /// </summary>
    public Optional<int> PremiumTier { get; }

    /// <summary>
    /// The amount of server boosts this guild has.
    /// </summary>
    public Optional<int> PremiumSubscriptionCount { get; }

    /// <summary>
    /// The preferred locale of a community guild, defaults to "en-US".
    /// </summary>
    public Optional<string> PreferredLocale { get; }

    /// <summary>
    /// The snowflake identifier of the channel where official notices from Discord are sent to.
    /// </summary>
    public Optional<Snowflake?> PublicUpdatesChannelId { get; }

    /// <summary>
    /// The maximum amount of users in a video channel.
    /// </summary>
    public Optional<int> MaxVideoChannelUsers { get; }

    /// <summary>
    /// The maximum amount of users in a stage video channel.
    /// </summary>
    public Optional<int> MaxStageVideoChannelUsers { get; }

    /// <summary>
    /// The approximate number of members in this guild.
    /// </summary>
    public Optional<int> ApproximateMemberCount { get; }

    /// <summary>
    /// The approximate number of non-offline members in this guild.
    /// </summary>
    public Optional<int> ApproximatePresenceCount { get; }

    /// <summary>
    /// The welcome screen of a community guild, shown to new members.
    /// </summary>
    public Optional<IWelcomeScreen> WelcomeScreen { get; }

    /// <summary>
    /// The NSFW level of this guild.
    /// </summary>
    public Optional<DiscordNsfwLevel> NsfwLevel { get; }

    /// <summary>
    /// The custom guild stickers.
    /// </summary>
    public Optional<IReadOnlyList<ISticker>> Stickers { get; }

    /// <summary>
    /// Indicates whether this guild has the boost progress bar enabled.
    /// </summary>
    public Optional<bool> PremiumProgressBarEnabled { get; }

    /// <summary>
    /// The snowflake identifier of the channel where community servers receive safety alerts.
    /// </summary>
    public Optional<Snowflake?> SafetyAlertsChannelId { get; }
}
