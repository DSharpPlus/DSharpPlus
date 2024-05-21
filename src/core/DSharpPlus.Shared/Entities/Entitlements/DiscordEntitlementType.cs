// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// The type of an entitlement.
/// </summary>
public enum DiscordEntitlementType
{
    /// <summary>
    /// The entitlement was purchased by the user.
    /// </summary>
    Purchase = 1,

    /// <summary>
    /// The entitlement was given for a Nitro subscription.
    /// </summary>
    PremiumSubscription,

    /// <summary>
    /// The entitlement was a gift from the developer.
    /// </summary>
    DeveloperGift,

    /// <summary>
    /// The entitlement was purchased by a developer in test mode.
    /// </summary>
    TestModePurchase,

    /// <summary>
    /// The entitlement was granted when the SKU was free.
    /// </summary>
    FreePurchase,

    /// <summary>
    /// The entitlement was gifted by another user.
    /// </summary>
    UserGift,

    /// <summary>
    /// The entitlement was claimed by the user as a Nitro subscriber.
    /// </summary>
    PremiumPurchase,

    /// <summary>
    /// This entitlement was purchased as an app subscription.
    /// </summary>
    ApplicationSubscription
}
