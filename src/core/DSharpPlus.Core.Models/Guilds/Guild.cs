// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IGuild" />
public sealed record Guild : IGuild
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public string? Icon { get; init; }

    /// <inheritdoc/>
    public Optional<string?> IconHash { get; init; }

    /// <inheritdoc/>
    public string? Splash { get; init; }

    /// <inheritdoc/>
    public string? DiscoverySplash { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Owner { get; init; }

    /// <inheritdoc/>
    public required Snowflake OwnerId { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPermissions> Permissions { get; init; }

    /// <inheritdoc/>
    public Snowflake? AfkChannelId { get; init; }

    /// <inheritdoc/>
    public required int AfkTimeout { get; init; }

    /// <inheritdoc/>
    public Optional<bool> WidgetEnabled { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> WidgetChannelId { get; init; }

    /// <inheritdoc/>
    public required DiscordVerificationLevel VerificationLevel { get; init; }

    /// <inheritdoc/>
    public required DiscordMessageNotificationLevel DefaultMessageNotifications { get; init; }

    /// <inheritdoc/>
    public required DiscordExplicitContentFilterLevel ExplicitContentFilter { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IRole> Roles { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IEmoji> Emojis { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<string> Features { get; init; }

    /// <inheritdoc/>
    public required DiscordMfaLevel MfaLevel { get; init; }

    /// <inheritdoc/>
    public Snowflake? ApplicationId { get; init; }

    /// <inheritdoc/>
    public Snowflake? SystemChannelId { get; init; }

    /// <inheritdoc/>
    public required DiscordSystemChannelFlags SystemChannelFlags { get; init; }

    /// <inheritdoc/>
    public Snowflake? RulesChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<int?> MaxPresences { get; init; }

    /// <inheritdoc/>
    public Optional<int> MaxMembers { get; init; }

    /// <inheritdoc/>
    public string? VanityUrlCode { get; init; }

    /// <inheritdoc/>
    public string? Description { get; init; }

    /// <inheritdoc/>
    public string? Banner { get; init; }

    /// <inheritdoc/>
    public required int PremiumTier { get; init; }

    /// <inheritdoc/>
    public Optional<int> PremiumSubscriptionCount { get; init; }

    /// <inheritdoc/>
    public required string PreferredLocale { get; init; }

    /// <inheritdoc/>
    public Snowflake? PublicUpdatesChannelId { get; init; }

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
    public required DiscordNsfwLevel NsfwLevel { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<ISticker>> Stickers { get; init; }

    /// <inheritdoc/>
    public required bool PremiumProgressBarEnabled { get; init; }

    /// <inheritdoc/>
    public Snowflake? SafetyAlertsChannelId { get; init; }
}