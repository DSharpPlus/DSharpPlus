// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IPartialSticker" />
public sealed record PartialSticker : IPartialSticker
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> PackId { get; init; }

    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Description { get; init; }

    /// <inheritdoc/>
    public Optional<string> Tags { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordStickerType> Type { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordStickerFormatType> FormatType { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Available { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<IUser> User { get; init; }

    /// <inheritdoc/>
    public Optional<int> SortValue { get; init; }
}