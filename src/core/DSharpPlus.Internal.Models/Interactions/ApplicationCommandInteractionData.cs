// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IApplicationCommandInteractionData" />
public sealed record ApplicationCommandInteractionData : IApplicationCommandInteractionData
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
    public Optional<IReadOnlyList<IApplicationCommandInteractionDataOption>> Options { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> TargetId { get; init; }
}
