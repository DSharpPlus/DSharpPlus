// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="ITemplate" />
public sealed record Template : ITemplate
{
    /// <inheritdoc/>
    public required string Code { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public string? Description { get; init; }

    /// <inheritdoc/>
    public required int UsageCount { get; init; }

    /// <inheritdoc/>
    public required Snowflake CreatorId { get; init; }

    /// <inheritdoc/>
    public required IUser Creator { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <inheritdoc/>
    public required DateTimeOffset UpdatedAt { get; init; }

    /// <inheritdoc/>
    public required Snowflake SourceGuildId { get; init; }

    /// <inheritdoc/>
    public required IPartialGuild SerializedSourceGuild { get; init; }

    /// <inheritdoc/>
    public bool? IsDirty { get; init; }
}