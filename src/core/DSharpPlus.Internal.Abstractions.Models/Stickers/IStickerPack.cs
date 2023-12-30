// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a pack of standard stickers.
/// </summary>
public interface IStickerPack
{
    /// <summary>
    /// The snowflake identifier of this sticker pack.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The stickers in this pack.
    /// </summary>
    public IReadOnlyList<ISticker> Stickers { get; }

    /// <summary>
    /// The name of this sticker pack.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The snowflake identifier of the pack's SKU.
    /// </summary>
    public Snowflake SkuId { get; }

    /// <summary>
    /// The snowflake of the sticker shown as the pack's icon.
    /// </summary>
    public Optional<Snowflake> CoverStickerId { get; }

    /// <summary>
    /// The description of this sticker pack.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The snowflake identifier of this pack's banner image.
    /// </summary>
    public Optional<Snowflake> BannerAssetId { get; }
}
