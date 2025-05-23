// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="ISeparatorComponent" />
public sealed record SeparatorComponent : ISeparatorComponent
{
    /// <inheritdoc/>
    public required DiscordMessageComponentType Type { get; init; }

    /// <inheritdoc/>
    public Optional<int> Id { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Divider { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordSeparatorSpacingSize> Spacing { get; init; }
}