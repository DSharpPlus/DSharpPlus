// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IActivityLocation" />
public sealed record ActivityLocation : IActivityLocation
{
    /// <inheritdoc/>
    public required string Id { get; init; }

    /// <inheritdoc/>
    public required string Kind { get; init; }

    /// <inheritdoc/>
    public required Snowflake ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> GuildId { get; init; }
}