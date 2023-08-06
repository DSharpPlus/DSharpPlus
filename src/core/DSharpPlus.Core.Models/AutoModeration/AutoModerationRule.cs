// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IAutoModerationRule" />
public sealed record AutoModerationRule : IAutoModerationRule
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required Snowflake GuildId { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required Snowflake CreatorId { get; init; }

    /// <inheritdoc/>
    public required DiscordAutoModerationEventType EventType { get; init; }

    /// <inheritdoc/>
    public required DiscordAutoModerationTriggerType TriggerType { get; init; }

    /// <inheritdoc/>
    public required IAutoModerationTriggerMetadata TriggerMetadata { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IAutoModerationAction> Actions { get; init; }

    /// <inheritdoc/>
    public required bool Enabled { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<Snowflake> ExemptRoles { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<Snowflake> ExemptChannels { get; init; }
}