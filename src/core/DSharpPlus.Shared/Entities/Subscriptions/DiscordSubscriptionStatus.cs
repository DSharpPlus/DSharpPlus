// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Indicates the status of a subscription object.
/// </summary>
public enum DiscordSubscriptionStatus
{
    /// <summary>
    /// Specifies that the subscription is active and will renew.
    /// </summary>
    Active,

    /// <summary>
    /// Specifies that the subscription is active and will not renew.
    /// </summary>
    Ending,

    /// <summary>
    /// Specifies that the subscription is inactive.
    /// </summary>
    Inactive
}
