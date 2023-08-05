// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IPartialRole" />
public sealed record PartialRole : IPartialRole
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<int> Color { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Hoist { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Hash { get; init; }

    /// <inheritdoc/>
    public Optional<string?> UnicodeEmoji { get; init; }

    /// <inheritdoc/>
    public Optional<int> Position { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPermissions> Permissions { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Managed { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Mentionable { get; init; }

    /// <inheritdoc/>
    public Optional<IRoleTags> Tags { get; init; }
}