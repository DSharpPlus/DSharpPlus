// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IRole" />
public sealed record Role : IRole
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required int Color { get; init; }

    /// <inheritdoc/>
    public required bool Hoist { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Hash { get; init; }

    /// <inheritdoc/>
    public Optional<string?> UnicodeEmoji { get; init; }

    /// <inheritdoc/>
    public required int Position { get; init; }

    /// <inheritdoc/>
    public required DiscordPermissions Permissions { get; init; }

    /// <inheritdoc/>
    public required bool Managed { get; init; }

    /// <inheritdoc/>
    public required bool Mentionable { get; init; }

    /// <inheritdoc/>
    public Optional<IRoleTags> Tags { get; init; }

    /// <inheritdoc/>
    public required DiscordRoleFlags Flags { get; init; }
}
