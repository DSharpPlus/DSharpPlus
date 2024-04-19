// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IButtonComponent" />
public sealed record ButtonComponent : IButtonComponent
{
    /// <inheritdoc/>
    public required DiscordMessageComponentType Type { get; init; }

    /// <inheritdoc/>
    public required DiscordButtonStyle Style { get; init; }

    /// <inheritdoc/>
    public Optional<string> Label { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialEmoji> Emoji { get; init; }

    /// <inheritdoc/>
    public Optional<string> CustomId { get; init; }

    /// <inheritdoc/>
    public Optional<string> Url { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Disabled { get; init; }
}
