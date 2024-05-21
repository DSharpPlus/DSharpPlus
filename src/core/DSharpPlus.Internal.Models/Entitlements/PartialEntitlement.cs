// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPartialEntitlement" />
public sealed record PartialEntitlement : IPartialEntitlement
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> SkuId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> UserId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> ApplicationId { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordEntitlementType> Type { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Deleted { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset> StartsAt { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset> EndsAt { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Consumed { get; init; }
}