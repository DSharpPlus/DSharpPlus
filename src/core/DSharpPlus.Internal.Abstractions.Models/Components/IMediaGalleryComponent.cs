// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a gallery of one to ten media items to display.
/// </summary>
public interface IMediaGalleryComponent : IComponent
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
    /// 1-10 media items.
    /// </summary>
    public IReadOnlyList<IMediaGalleryItem> Items { get; }
}
