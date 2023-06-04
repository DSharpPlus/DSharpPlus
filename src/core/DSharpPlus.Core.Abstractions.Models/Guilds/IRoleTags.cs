// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Contains additional tags associated with a given role.
/// </summary>
public interface IRoleTags
{
    /// <summary>
    /// The snowflake identifier of the bot this role belongs to.
    /// </summary>
    public Optional<Snowflake> BotId { get; }

    /// <summary>
    /// The snowflake identifier of the integration this role belongs to.
    /// </summary>
    public Optional<Snowflake> IntegrationId { get; }

    /// <summary>
    /// Indicates whether this is the guild's booster role.
    /// </summary>
    public bool PremiumSubscriber { get; }

    /// <summary>
    /// The snowflake identifier of this role's subscription SKU and listing.
    /// </summary>
    public Optional<Snowflake> SubscriptionListingId { get; }

    /// <summary>
    /// Indicates whether this role is available for purchase.
    /// </summary>
    public bool AvailableForPurchase { get; }

    /// <summary>
    /// Indicates whether this role is a linked role.
    /// </summary>
    public bool GuildConnections { get; }
}
