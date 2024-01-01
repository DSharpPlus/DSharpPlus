// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="ICreateTestEntitlementPayload" />
public sealed record CreateTestEntitlementPayload : ICreateTestEntitlementPayload
{
    /// <inheritdoc/>
    public required Snowflake SkuId { get; init; }

    /// <inheritdoc/>
    public required Snowflake OwnerId { get; init; }

    /// <inheritdoc/>
    public required DiscordEntitlementOwnerType OwnerType { get; init; }
}