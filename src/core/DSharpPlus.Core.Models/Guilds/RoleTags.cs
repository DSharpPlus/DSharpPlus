// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IRoleTags" />
public sealed record RoleTags : IRoleTags
{
    /// <inheritdoc/>
    public Optional<Snowflake> BotId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> IntegrationId { get; init; }

    /// <inheritdoc/>
    public required bool PremiumSubscriber { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> SubscriptionListingId { get; init; }

    /// <inheritdoc/>
    public required bool AvailableForPurchase { get; init; }

    /// <inheritdoc/>
    public required bool GuildConnections { get; init; }
}