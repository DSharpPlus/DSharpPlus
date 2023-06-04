// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a thumbnail in an embed.
/// </summary>
public interface IEmbedThumbnail
{
    /// <summary>
    /// The source URL of this thumbnail.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// The proxied URL of this thumbnail.
    /// </summary>
    public Optional<string> ProxyUrl { get; }

    /// <summary>
    /// The height of the thumbnail.
    /// </summary>
    public Optional<int> Height { get; }

    /// <summary>
    /// The width of the thumbnail.
    /// </summary>
    public Optional<int> Width { get; }
}
