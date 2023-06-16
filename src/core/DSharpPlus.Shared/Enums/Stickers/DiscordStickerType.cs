// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Specifies whether the given sticker belongs to a guild or is a default sticker.
/// </summary>
public enum DiscordStickerType
{
    /// <summary>
    /// Indicates an official sticker in a sticker pack; a nitro sticker or a part of
    /// a removed purchasable pack.
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Indicates a sticker uploaded to a guild.
    /// </summary>
    Guild
}
