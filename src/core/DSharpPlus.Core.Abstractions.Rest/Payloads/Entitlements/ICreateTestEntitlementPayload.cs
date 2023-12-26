// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /applications/:application-id/entitlements</c>.
/// </summary>
public interface ICreateTestEntitlementPayload
{
    /// <summary>
    /// The identifier of the string to grant the entitlement to.
    /// </summary>
    public Snowflake SkuId { get; }

    /// <summary>
    /// The identifier of the guild or user to grant the entitlement to.
    /// </summary>
    public Snowflake OwnerId { get; }

    /// <summary>
    /// Specifies what kind of entity the owner is.
    /// </summary>
    public DiscordEntitlementOwnerType OwnerType { get; }
}
