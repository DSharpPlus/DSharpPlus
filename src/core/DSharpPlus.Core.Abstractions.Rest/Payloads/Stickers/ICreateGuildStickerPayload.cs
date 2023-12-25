// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Rpresents a payload to <c>POST /guilds/:guild-id/stickers</c>.
/// </summary>
public interface ICreateGuildStickerPayload
{
    /// <summary>
    /// The name of this sticker.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description for this sticker.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The autocomplete suggestion tags for this sticker.
    /// </summary>
    public string Tags { get; }

    /// <summary>
    /// File contents of the sticker to upload.
    /// </summary>
    public AttachmentData File { get; }
}
