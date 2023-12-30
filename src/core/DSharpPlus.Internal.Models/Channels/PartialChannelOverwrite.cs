// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPartialChannelOverwrite" />
public sealed record PartialChannelOverwrite : IPartialChannelOverwrite
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordChannelOverwriteType> Type { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPermissions> Allow { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPermissions> Deny { get; init; }
}