// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a single media item inside a media gallery.
/// </summary>
public interface IMediaGalleryItem
{
    /// <summary>
    /// The underlying media item to display.
    /// </summary>
    public IUnfurledMediaItem Media { get; }

    /// <summary>
    /// Alt text for this media item.
    /// </summary>
    public Optional<bool> Description { get; }

    /// <summary>
    /// Indicates whether this media item should be spoilered; that is, blurred out and only showed on click or if the user automatically shows all spoilers.
    /// </summary>
    public Optional<bool> Spoiler { get; }
}
