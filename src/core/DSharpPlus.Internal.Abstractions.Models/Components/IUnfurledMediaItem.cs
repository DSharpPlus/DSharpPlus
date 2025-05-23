// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents an attachment reference for use in components.
/// </summary>
public interface IUnfurledMediaItem
{
    /// <summary>
    /// The URL to the attachment; supports arbitrary URLs and <c>attachemnt://</c> references to files uploaded along the message.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// The proxied URL of this media item.
    /// </summary>
    public Optional<string> ProxyUrl { get; }

    /// <summary>
    /// The height of this media item.
    /// </summary>
    public Optional<int?> Height { get; }

    /// <summary>
    /// The width of this media item.
    /// </summary>
    public Optional<int?> Width { get; }

    /// <summary>
    /// The MIME/content type of the content.
    /// </summary>
    public Optional<string> ContentType { get; }
}
