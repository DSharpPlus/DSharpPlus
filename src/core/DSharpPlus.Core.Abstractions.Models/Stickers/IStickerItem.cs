// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Contains the information needed to render a sticker.
/// </summary>
public interface IStickerItem
{
    /// <summary>
    /// The snowflake identifier of the sticker
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The name of this sticker.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The file format of this sticker.
    /// </summary>
    public DiscordStickerFormatType FormatType { get; }
}
