// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Contains metadata about a role subscription.
/// </summary>
public interface IRoleSubscriptionData
{
    /// <summary>
    /// The snowflake identifier of the SKU and listing that the user is subscribed to.
    /// </summary>
    public Snowflake RoleSubscriptionListingId { get; }

    /// <summary>
    /// The name of the tier that the user is subscribed to.
    /// </summary>
    public string TierName { get; }

    /// <summary>
    /// The cumulative number of months that the user has been subscribed for.
    /// </summary>
    public int TotalMonthsSubscribed { get; }

    /// <summary>
    /// Indicates whether this notification is for a renewal, rather than a new purchase.
    /// </summary>
    public bool IsRenewal { get; }
}
