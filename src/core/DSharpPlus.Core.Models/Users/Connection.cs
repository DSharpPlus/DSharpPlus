// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IConnection" />
public sealed record Connection : IConnection
{
    /// <inheritdoc/>
    public required string Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required string Type { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Revoked { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IPartialIntegration>> Integrations { get; init; }

    /// <inheritdoc/>
    public required bool Verified { get; init; }

    /// <inheritdoc/>
    public required bool FriendSync { get; init; }

    /// <inheritdoc/>
    public required bool ShowActivity { get; init; }

    /// <inheritdoc/>
    public required bool TwoWayLink { get; init; }

    /// <inheritdoc/>
    public required DiscordConnectionVisibility Visibility { get; init; }
}