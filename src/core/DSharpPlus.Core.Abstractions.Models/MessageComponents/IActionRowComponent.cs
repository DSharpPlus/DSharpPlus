// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a container component for other components.
/// </summary>
public interface IActionRowComponent
{
    /// <summary>
    /// <seealso cref="DiscordComponentType.ActionRow"/>
    /// </summary>
    public DiscordComponentType Type { get; }

    /// <summary>
    /// The child components of this action row: up to five buttons, or one non-button component.
    /// </summary>
    public IReadOnlyList<IInteractiveComponent> Components { get; }
}
