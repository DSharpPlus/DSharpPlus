// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Abstractions.Rest.Responses;

/// <summary>
/// Represents a response from <c>GET /sticker-packs</c>.
/// </summary>
public readonly record struct ListStickerPacksResponse
{
    /// <summary>
    /// The sticker packs returned by the call.
    /// </summary>
    public required IReadOnlyList<IStickerPack> StickerPacks { get; init; }
}
