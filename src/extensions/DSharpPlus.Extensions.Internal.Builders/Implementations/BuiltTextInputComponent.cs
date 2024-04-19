// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Extensions.Internal.Builders.Implementations;

/// <inheritdoc cref="ITextInputComponent" />
internal sealed record BuiltTextInputComponent : ITextInputComponent
{
    /// <inheritdoc/>
    public required DiscordMessageComponentType Type { get; init; }

    /// <inheritdoc/>
    public required string CustomId { get; init; }

    /// <inheritdoc/>
    public required DiscordTextInputStyle Style { get; init; }

    /// <inheritdoc/>
    public required string Label { get; init; }

    /// <inheritdoc/>
    public Optional<int> MinLength { get; init; }

    /// <inheritdoc/>
    public Optional<int> MaxLength { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Required { get; init; }

    /// <inheritdoc/>
    public Optional<string> Value { get; init; }

    /// <inheritdoc/>
    public Optional<string> Placeholder { get; init; }
}
