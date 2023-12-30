// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPartialAutoModerationRule" />
public sealed record PartialAutoModerationRule : IPartialAutoModerationRule
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> CreatorId { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordAutoModerationEventType> EventType { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordAutoModerationTriggerType> TriggerType { get; init; }

    /// <inheritdoc/>
    public Optional<IAutoModerationTriggerMetadata> TriggerMetadata { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IAutoModerationAction>> Actions { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Enabled { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> ExemptRoles { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> ExemptChannels { get; init; }
}