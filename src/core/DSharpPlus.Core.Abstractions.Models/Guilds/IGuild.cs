// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a fully populated guild object.
/// </summary>
public interface IGuild : IPartialGuild
{
    /// <inheritdoc cref="IPartialGuild.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialGuild.Name"/>
    public new string Name { get; }

    /// <inheritdoc cref="IPartialGuild.Name"/>
    public new string? Icon { get; }

    /// <inheritdoc cref="IPartialGuild.Splash"/>
    public new string? Splash { get; }

    /// <inheritdoc cref="IPartialGuild.DiscoverySplash"/>
    public new string? DiscoverySplash { get; }

    /// <inheritdoc cref="IPartialGuild.OwnerId"/>
    public new Snowflake OwnerId { get; }

    /// <inheritdoc cref="IPartialGuild.AfkChannelId"/>
    public new Snowflake? AfkChannelId { get; }

    /// <inheritdoc cref="IPartialGuild.AfkTimeout"/>
    public new int AfkTimeout { get; }

    /// <inheritdoc cref="IPartialGuild.VerificationLevel"/>
    public new DiscordVerificationLevel VerificationLevel { get; }

    /// <inheritdoc cref="IPartialGuild.DefaultMessageNotifications"/>
    public new DiscordMessageNotificationLevel DefaultMessageNotifications { get; }

    /// <inheritdoc cref="IPartialGuild.ExplicitContentFilter"/>
    public new DiscordExplicitContentFilterLevel ExplicitContentFilter { get; }

    /// <inheritdoc cref="IPartialGuild.Roles"/>
    public new IReadOnlyList<IRole> Roles { get; }

    /// <inheritdoc cref="IPartialGuild.Emojis"/>
    public new IReadOnlyList<IEmoji> Emojis { get; }

    /// <inheritdoc cref="IPartialGuild.Features"/>
    public new IReadOnlyList<string> Features { get; }

    /// <inheritdoc cref="IPartialGuild.MfaLevel"/>
    public new DiscordMfaLevel MfaLevel { get; }

    /// <inheritdoc cref="IPartialGuild.ApplicationId"/>
    public new Snowflake? ApplicationId { get; }

    /// <inheritdoc cref="IPartialGuild.SystemChannelId"/>
    public new Snowflake? SystemChannelId { get; }

    /// <inheritdoc cref="IPartialGuild.SystemChannelFlags"/>
    public new DiscordSystemChannelFlags SystemChannelFlags { get; }

    /// <inheritdoc cref="IPartialGuild.RulesChannelId"/>
    public new Snowflake? RulesChannelId { get; }

    /// <inheritdoc cref="IPartialGuild.VanityUrlCode"/>
    public new string? VanityUrlCode { get; }

    /// <inheritdoc cref="IPartialGuild.Description"/>
    public new string? Description { get; }

    /// <inheritdoc cref="IPartialGuild.Banner"/>
    public new string? Banner { get; }

    /// <inheritdoc cref="IPartialGuild.PremiumTier"/>
    public new int PremiumTier { get; }

    /// <inheritdoc cref="IPartialGuild.PreferredLocale"/>
    public new string PreferredLocale { get; }

    /// <inheritdoc cref="IPartialGuild.PublicUpdatesChannelId"/>
    public new Snowflake? PublicUpdatesChannelId { get; }

    /// <inheritdoc cref="IPartialGuild.NsfwLevel"/>
    public new DiscordNsfwLevel NsfwLevel { get; }

    /// <inheritdoc cref="IPartialGuild.PremiumProgressBarEnabled"/>
    public new bool PremiumProgressBarEnabled { get; }

    /// <inheritdoc cref="IPartialGuild.SafetyAlertsChannelId"/>
    public new Snowflake? SafetyAlertsChannelId { get; }

    // routes for partial access

    /// <inheritdoc/>
    Optional<Snowflake> IPartialGuild.Id => this.Id;

    /// <inheritdoc/>
    Optional<string> IPartialGuild.Name => this.Name;

    /// <inheritdoc/>
    Optional<string?> IPartialGuild.Icon => this.Icon;

    /// <inheritdoc/>
    Optional<string?> IPartialGuild.Splash => this.Splash;

    /// <inheritdoc/>
    Optional<string?> IPartialGuild.DiscoverySplash => this.DiscoverySplash;

    /// <inheritdoc/>
    Optional<Snowflake> IPartialGuild.OwnerId => this.OwnerId;

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialGuild.AfkChannelId => this.AfkChannelId;

    /// <inheritdoc/>
    Optional<int> IPartialGuild.AfkTimeout => this.AfkTimeout;

    /// <inheritdoc/>
    Optional<DiscordVerificationLevel> IPartialGuild.VerificationLevel => this.VerificationLevel;

    /// <inheritdoc/>
    Optional<DiscordMessageNotificationLevel> IPartialGuild.DefaultMessageNotifications => this.DefaultMessageNotifications;

    /// <inheritdoc/>
    Optional<DiscordExplicitContentFilterLevel> IPartialGuild.ExplicitContentFilter => this.ExplicitContentFilter;

    /// <inheritdoc/>
    Optional<IReadOnlyList<IRole>> IPartialGuild.Roles => new(this.Roles);

    /// <inheritdoc/>
    Optional<IReadOnlyList<IEmoji>> IPartialGuild.Emojis => new(this.Emojis);

    /// <inheritdoc/>
    Optional<IReadOnlyList<string>> IPartialGuild.Features => new(this.Features);

    /// <inheritdoc/>
    Optional<DiscordMfaLevel> IPartialGuild.MfaLevel => this.MfaLevel;

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialGuild.ApplicationId => this.ApplicationId;

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialGuild.SystemChannelId => this.SystemChannelId;

    /// <inheritdoc/>
    Optional<DiscordSystemChannelFlags> IPartialGuild.SystemChannelFlags => this.SystemChannelFlags;

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialGuild.RulesChannelId => this.RulesChannelId;

    /// <inheritdoc/>
    Optional<string?> IPartialGuild.VanityUrlCode => this.VanityUrlCode;

    /// <inheritdoc/>
    Optional<string?> IPartialGuild.Description => this.Description;

    /// <inheritdoc/>
    Optional<string?> IPartialGuild.Banner => this.Banner;

    /// <inheritdoc/>
    Optional<int> IPartialGuild.PremiumTier => this.PremiumTier;

    /// <inheritdoc/>
    Optional<string> IPartialGuild.PreferredLocale => this.PreferredLocale;

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialGuild.PublicUpdatesChannelId => this.PublicUpdatesChannelId;

    /// <inheritdoc/>
    Optional<DiscordNsfwLevel> IPartialGuild.NsfwLevel => this.NsfwLevel;

    /// <inheritdoc/>
    Optional<bool> IPartialGuild.PremiumProgressBarEnabled => this.PremiumProgressBarEnabled;

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialGuild.SafetyAlertsChannelId => this.SafetyAlertsChannelId;
}
