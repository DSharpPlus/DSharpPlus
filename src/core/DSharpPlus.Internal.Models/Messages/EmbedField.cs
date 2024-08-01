// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IEmbedField" />
public sealed record EmbedField : IEmbedField
{
    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required string Value { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Inline { get; init; }
}