// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a subscription to an <see cref="ISku"/>.
/// </summary>
public interface ISubscription
{
    /// <summary>
    /// The snowflake identifier of this subscription.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The snowflake identifier of the subscribing user.
    /// </summary>
    public Snowflake UserId { get; }

    /// <summary>
    /// The list of SKUs this subscription applies to.
    /// </summary>
    public IReadOnlyList<Snowflake> SkuIds { get; }

    /// <summary>
    /// The list of entitlements this subscription applies to.
    /// </summary>
    public IReadOnlyList<Snowflake> EntitlementIds { get; }

    /// <summary>
    /// The starting timestamp of the current subscription period.
    /// </summary>
    public DateTimeOffset CurrentPeriodStart { get; }

    /// <summary>
    /// The ending timestamp of the current subscription period.
    /// </summary>
    public DateTimeOffset CurrentPeriodEnd { get; }

    /// <summary>
    /// Specifies the status of this subscription.
    /// </summary>
    public DiscordSubscriptionStatus Status { get; }

    /// <summary>
    /// Indicates when the subscription was cancelled, if applicable.
    /// </summary>
    public DateTimeOffset? CanceledAt { get; }

    /// <summary>
    /// Specifies the ISO3166-1-alpha-2 country code of the payment source used to purchase the subscription.
    /// Missing unless queried with a private OAuth2 scope.
    /// </summary>
    public Optional<string> Country { get; }
}
