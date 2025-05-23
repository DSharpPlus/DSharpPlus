// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// A top-level layout component that allows combining text with an accessory.
/// </summary>
public interface ISectionComponent : IComponent
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
    /// One to three text components to send within this section.
    /// </summary>
    public IReadOnlyList<IComponent> Components { get; }

    /// <summary>
    /// A thumbnail or button component to group with the text in this component.
    /// </summary>
    public IComponent Accessory { get; }
}
