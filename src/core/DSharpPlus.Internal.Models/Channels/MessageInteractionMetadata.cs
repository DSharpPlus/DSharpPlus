// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IMessageInteractionMetadata" />
public sealed record MessageInteractionMetadata : IMessageInteractionMetadata
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required DiscordInteractionType Type { get; init; }

    /// <inheritdoc/>
    public required Snowflake UserId { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyDictionary<DiscordApplicationIntegrationType, Snowflake> AuthorizingIntegrationOwners { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> OriginalResponseMessageId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> InteractedMessageId { get; init; }

    /// <inheritdoc/>
    public Optional<IMessageInteractionMetadata> TriggeringInteractionMetadata { get; init; }
}
