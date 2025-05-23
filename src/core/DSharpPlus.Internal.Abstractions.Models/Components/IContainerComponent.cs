// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// A top-level layout-only component that is visually distinct from the background and has a customizable colour bar akin to an embed.
/// </summary>
public interface IContainerComponent : IComponent
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
    /// The components to display in this container.
    /// </summary>
    public IReadOnlyList<IComponent> Components { get; }

    /// <summary>
    /// The colour for the colour bar. Unset blends it with the container's background, which depends on client theme.
    /// </summary>
    public Optional<int?> AccentColor { get; }

    /// <summary>
    /// Indicates whether the container should be spoilered, that is, blurred out until clicked or if the user automatically shows all spoilers.
    /// Defaults to false.
    /// </summary>
    public Optional<bool> Spoiler { get; }
}
