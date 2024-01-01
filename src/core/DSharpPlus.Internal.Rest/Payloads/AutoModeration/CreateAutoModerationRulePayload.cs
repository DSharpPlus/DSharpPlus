// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="ICreateAutoModerationRulePayload" />
public sealed record CreateAutoModerationRulePayload : ICreateAutoModerationRulePayload
{
    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required DiscordAutoModerationEventType EventType { get; init; }

    /// <inheritdoc/>
    public required DiscordAutoModerationTriggerType TriggerType { get; init; }

    /// <inheritdoc/>
    public Optional<IAutoModerationTriggerMetadata> TriggerMetadata { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IAutoModerationAction> Actions { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Enabled { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> ExemptRoles { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> ExemptChannels { get; init; }
}