// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an entitlement to a premium offering in an application.
/// </summary>
public interface IEntitlement
{
    /// <summary>
    /// The snowflake identifier of this entitlement.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The snowflake identifier of the SKU.
    /// </summary>
    public Snowflake SkuId { get; }

    /// <summary>
    /// The snowflake identifier of the user that is granted access to the entitlement's SKU.
    /// </summary>
    public Optional<Snowflake> UserId { get; }

    /// <summary>
    /// The snowflake identifier of the guild that is granted access to the entitlement's SKU.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// The snowflake identifier of the parent application.
    /// </summary>
    public Snowflake ApplicationId { get; }

    /// <summary>
    /// The type of this entitlement.
    /// </summary>
    public EntitlementType Type { get; }

    /// <summary>
    /// <c>false</c>.
    /// </summary>
    public bool Consumed { get; }

    /// <summary>
    /// The starting date at which this entitlement is valid. Not present when using test entitlements.
    /// </summary>
    public Optional<DateTimeOffset> StartsAt { get; }

    /// <summary>
    /// The date at which this entitlement is no longer valid. Not present when using test entitlements.
    /// </summary>
    public Optional<DateTimeOffset> EndsAt { get; }
}
