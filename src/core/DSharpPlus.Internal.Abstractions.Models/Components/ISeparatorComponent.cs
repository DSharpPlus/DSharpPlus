// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// A top-level component that adds vertical padding and/or visual division between components.
/// </summary>
public interface ISeparatorComponent : IComponent
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
    /// Indicates whether a visual divider should be displayed in this component. Defaults to true.
    /// </summary>
    public Optional<bool> Divider { get; }

    /// <summary>
    /// The amount of space this separator takes up.
    /// </summary>
    public Optional<DiscordSeparatorSpacingSize> Spacing { get; }
}
