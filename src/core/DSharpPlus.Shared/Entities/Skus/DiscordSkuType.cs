// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the type of a given SKU.
/// </summary>
public enum DiscordSkuType
{
    /// <summary>
    /// Represents a recurring subscription.
    /// </summary>
    Subscription = 5,

    /// <summary>
    /// A system-generated group for each subscription SKU created.
    /// </summary>
    SubscriptionGroup = 6
}
