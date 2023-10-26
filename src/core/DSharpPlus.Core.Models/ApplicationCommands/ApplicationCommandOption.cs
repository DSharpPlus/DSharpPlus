// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

using OneOf;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IApplicationCommandOption" />
public sealed record ApplicationCommandOption : IApplicationCommandOption
{
    /// <inheritdoc/>
    public required DiscordApplicationCommandOptionType Type { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; init; }

    /// <inheritdoc/>
    public required string Description { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<string, string>?> DescriptionLocalizations { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Required { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IApplicationCommandOptionChoice>> Choices { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IApplicationCommandOption>> Options { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<DiscordChannelType>> ChannelTypes { get; init; }

    /// <inheritdoc/>
    public Optional<OneOf<int, double>> MinValue { get; init; }

    /// <inheritdoc/>
    public Optional<OneOf<int, double>> MaxValue { get; init; }

    /// <inheritdoc/>
    public Optional<int> MinLength { get; init; }

    /// <inheritdoc/>
    public Optional<int> MaxLength { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Autocomplete { get; init; }
}