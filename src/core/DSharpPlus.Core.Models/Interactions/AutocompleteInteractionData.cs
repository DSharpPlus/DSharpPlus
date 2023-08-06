// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IAutocompleteInteractionData" />
public sealed record AutocompleteInteractionData : IAutocompleteInteractionData
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required DiscordApplicationCommandType Type { get; init; }

    /// <inheritdoc/>
    public Optional<IResolvedData> Resolved { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IAutocompleteInteractionDataOption>> Options { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }
}