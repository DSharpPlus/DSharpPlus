// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a container component for other components.
/// </summary>
public interface IActionRowComponent : IComponent
{
    /// <summary>
    /// <see cref="DiscordMessageComponentType.ActionRow"/>
    /// </summary>
    public DiscordMessageComponentType Type { get; }

    /// <summary>
    /// An optional numeric identifier for this component.
    /// </summary>
    public Optional<int> Id { get; }

    /// <summary>
    /// The child components of this action row: up to five buttons, or one non-button component.
    /// </summary>
    public IReadOnlyList<IComponent> Components { get; }
}
