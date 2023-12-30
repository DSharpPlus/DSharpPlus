// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IChannelSelectComponent" />
public sealed record ChannelSelectComponent : IChannelSelectComponent
{
    /// <inheritdoc/>
    public required DiscordMessageComponentType Type { get; init; }

    /// <inheritdoc/>
    public required string CustomId { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<DiscordChannelType> ChannelTypes { get; init; }

    /// <inheritdoc/>
    public Optional<string> Placeholder { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IDefaultSelectValue>> DefaultValues { get; init; }

    /// <inheritdoc/>
    public Optional<int> MinValues { get; init; }

    /// <inheritdoc/>
    public Optional<int> MaxValues { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Disabled { get; init; }
}