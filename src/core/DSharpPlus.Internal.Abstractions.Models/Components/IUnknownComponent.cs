// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Specifies a component of unknown type.
/// </summary>
public interface IUnknownComponent : IComponent
{
    /// <summary>
    /// Gets the type of this component. You should expect to handle values not specified by the enum type.
    /// </summary>
    public DiscordMessageComponentType Type { get; }

    /// <summary>
    /// An optional numeric identifier for this component.
    /// </summary>
    public Optional<int> Id { get; }

    /// <summary>
    /// Gets the raw string represented by this component.
    /// </summary>
    public string RawPayload { get; }
}
