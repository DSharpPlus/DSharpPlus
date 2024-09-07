// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="ISubscription" />
public sealed record Subscription : ISubscription
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required Snowflake UserId { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<Snowflake> SkuIds { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<Snowflake> EntitlementIds { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset CurrentPeriodStart { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset CurrentPeriodEnd { get; init; }

    /// <inheritdoc/>
    public required DiscordSubscriptionStatus Status { get; init; }

    /// <inheritdoc/>
    public DateTimeOffset? CanceledAt { get; init; }

    /// <inheritdoc/>
    public Optional<string> Country { get; init; }
}