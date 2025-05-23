// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

public interface IThumbnailComponent : IComponent
{
    /// <summary>
    /// The type of this component.
    /// </summary>
    public DiscordMessageComponentType Type { get; }

    /// <summary>
    /// An optional numeric identifier for this component.
    /// </summary>
    public Optional<int> Id { get; }

    /// <summary>
    /// The media item to be displayed in this thumbnail.
    /// </summary>
    public IUnfurledMediaItem Media { get; }

    /// <summary>
    /// Alt text for this thumbnail.
    /// </summary>
    public Optional<bool> Description { get; }

    /// <summary>
    /// Indicates whether this thumbnail should be spoilered; that is, blurred out and only showed on click or if the user automatically shows all spoilers.
    /// </summary>
    public Optional<bool> Spoiler { get; }
}
