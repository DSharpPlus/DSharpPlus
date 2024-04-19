// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyGuildPayload" />
public sealed record ModifyGuildPayload : IModifyGuildPayload
{
    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordVerificationLevel?> VerificationLevel { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordMessageNotificationLevel?> DefaultMessageNotifications { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordExplicitContentFilterLevel?> ExplicitContentFilter { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> AfkChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<int> AfkTimeout { get; init; }

    /// <inheritdoc/>
    public Optional<ImageData?> Icon { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> OwnerId { get; init; }

    /// <inheritdoc/>
    public Optional<ImageData?> Splash { get; init; }

    /// <inheritdoc/>
    public Optional<ImageData?> DiscoverySplash { get; init; }

    /// <inheritdoc/>
    public Optional<ImageData?> Banner { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> SystemChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordSystemChannelFlags> SystemChannelFlags { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> RulesChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> PublicUpdatesChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<string?> PreferredLocale { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<string>> Features { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Description { get; init; }

    /// <inheritdoc/>
    public Optional<bool> PremiumProgressBarEnabled { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> SafetyAlertsChannelId { get; init; }
}
