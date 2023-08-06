// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IEmoji" />
public sealed record Emoji : IEmoji
{
    /// <inheritdoc/>
    public required Snowflake? Id { get; init; }

    /// <inheritdoc/>
    public required string? Name { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> Roles { get; init; }

    /// <inheritdoc/>
    public Optional<IUser> User { get; init; }

    /// <inheritdoc/>
    public Optional<bool> RequireColons { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Managed { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Animated { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Available { get; init; }
}