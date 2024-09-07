// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IMessageSnapshotContent" />
public sealed record MessageSnapshotContent : IMessageSnapshotContent
{
    /// <inheritdoc/>
    public Optional<string> Content { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset> Timestamp { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> EditedTimestamp { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IUser>> Mentions { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> MentionRoles { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IAttachment>> Attachments { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IEmbed>> Embeds { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordMessageType> Type { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordMessageFlags> Flags { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IComponent>> Components { get; init; }

    /// <inheritdoc/>
    public Optional<IStickerItem> StickerItems { get; init; }
}