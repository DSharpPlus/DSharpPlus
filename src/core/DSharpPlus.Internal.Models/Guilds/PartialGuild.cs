// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPartialGuild" />
public sealed record PartialGuild : IPartialGuild
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Icon { get; init; }

    /// <inheritdoc/>
    public Optional<string?> IconHash { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Splash { get; init; }

    /// <inheritdoc/>
    public Optional<string?> DiscoverySplash { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Owner { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> OwnerId { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPermissions> Permissions { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> AfkChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<int> AfkTimeout { get; init; }

    /// <inheritdoc/>
    public Optional<bool> WidgetEnabled { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> WidgetChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordVerificationLevel> VerificationLevel { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordMessageNotificationLevel> DefaultMessageNotifications { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordExplicitContentFilterLevel> ExplicitContentFilter { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IRole>> Roles { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IEmoji>> Emojis { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<string>> Features { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordMfaLevel> MfaLevel { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> ApplicationId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> SystemChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordSystemChannelFlags> SystemChannelFlags { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> RulesChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<int?> MaxPresences { get; init; }

    /// <inheritdoc/>
    public Optional<int> MaxMembers { get; init; }

    /// <inheritdoc/>
    public Optional<string?> VanityUrlCode { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Description { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Banner { get; init; }

    /// <inheritdoc/>
    public Optional<int> PremiumTier { get; init; }

    /// <inheritdoc/>
    public Optional<int> PremiumSubscriptionCount { get; init; }

    /// <inheritdoc/>
    public Optional<string> PreferredLocale { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> PublicUpdatesChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<int> MaxVideoChannelUsers { get; init; }

    /// <inheritdoc/>
    public Optional<int> MaxStageVideoChannelUsers { get; init; }

    /// <inheritdoc/>
    public Optional<int> ApproximateMemberCount { get; init; }

    /// <inheritdoc/>
    public Optional<int> ApproximatePresenceCount { get; init; }

    /// <inheritdoc/>
    public Optional<IWelcomeScreen> WelcomeScreen { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordNsfwLevel> NsfwLevel { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<ISticker>> Stickers { get; init; }

    /// <inheritdoc/>
    public Optional<bool> PremiumProgressBarEnabled { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> SafetyAlertsChannelId { get; init; }
}