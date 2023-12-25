// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /guilds/:guild-id/stickers/:sticker-id</c>.
/// </summary>
public interface IModifyGuildStickerPayload
{
    /// <summary>
    /// The name of this sticker.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The description for this sticker.
    /// </summary>
    public Optional<string?> Description { get; }

    /// <summary>
    /// The autocomplete suggestion tags for this sticker.
    /// </summary>
    public Optional<string> Tags { get; }
}
