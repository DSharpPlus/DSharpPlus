// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IGuildWidget" />
public sealed record GuildWidget : IGuildWidget
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public string? InstantInvite { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IPartialChannel> Channels { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IPartialUser> Members { get; init; }

    /// <inheritdoc/>
    public required int PresenceCount { get; init; }
}