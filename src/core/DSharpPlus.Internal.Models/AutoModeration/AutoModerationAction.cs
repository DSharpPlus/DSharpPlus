// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IAutoModerationAction" />
public sealed record AutoModerationAction : IAutoModerationAction
{
    /// <inheritdoc/>
    public required DiscordAutoModerationActionType Type { get; init; }

    /// <inheritdoc/>
    public Optional<IAutoModerationActionMetadata> Metadata { get; init; }
}