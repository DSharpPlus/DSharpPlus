// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /applications/:application-id/emojis/:emoji-id</c>
/// </summary>
public interface IModifyApplicationEmojiPayload
{
    /// <summary>
    /// The new name of the emoji.
    /// </summary>
    public string Name { get; }
}
