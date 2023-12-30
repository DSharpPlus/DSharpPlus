// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a discord sticker that can be sent in messages.
/// </summary>
public interface ISticker : IPartialSticker
{
    /// <inheritdoc cref="IPartialSticker.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialSticker.Name"/>
    public new string Name { get; }

    /// <inheritdoc cref="IPartialSticker.Description"/>
    public new string? Description { get; }

    /// <inheritdoc cref="IPartialSticker.Tags"/>
    public new string Tags { get; }

    /// <inheritdoc cref="IPartialSticker.Type"/>
    public new DiscordStickerType Type { get; }

    /// <inheritdoc cref="IPartialSticker.FormatType"/>
    public new DiscordStickerFormatType FormatType { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<Snowflake> IPartialSticker.Id => this.Id;

    /// <inheritdoc/>
    Optional<string> IPartialSticker.Name => this.Name;

    /// <inheritdoc/>
    Optional<string?> IPartialSticker.Description => this.Description;

    /// <inheritdoc/>
    Optional<string> IPartialSticker.Tags => this.Tags;

    /// <inheritdoc/>
    Optional<DiscordStickerType> IPartialSticker.Type => this.Type;

    /// <inheritdoc/>
    Optional<DiscordStickerFormatType> IPartialSticker.FormatType => this.FormatType;
}
