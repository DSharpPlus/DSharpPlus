// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /applications/:application-id/emojis</c>.
/// </summary>
public interface ICreateApplicationEmojiPayload
{
    /// <summary>
    /// The name of the new emoji.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The 128x128 emoji image.
    /// </summary>
    public ImageData Image { get; }
}
