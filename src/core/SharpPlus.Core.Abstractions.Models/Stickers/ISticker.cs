// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="ISticker" />
public sealed record Sticker : ISticker
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> PackId { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required string? Description { get; init; }

    /// <inheritdoc/>
    public required string Tags { get; init; }

    /// <inheritdoc/>
    public required DiscordStickerType Type { get; init; }

    /// <inheritdoc/>
    public required DiscordStickerFormatType FormatType { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Available { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<IUser> User { get; init; }

    /// <inheritdoc/>
    public Optional<int> SortValue { get; init; }
}