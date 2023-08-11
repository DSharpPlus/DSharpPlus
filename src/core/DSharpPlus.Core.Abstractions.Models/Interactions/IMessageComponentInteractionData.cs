// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents metadata for a message component interaction.
/// </summary>
public interface IMessageComponentInteractionData
{
    /// <summary>
    /// The developer-defined ID of the component.
    /// </summary>
    public string CustomId { get; }

    /// <summary>
    /// The type of this component.
    /// </summary>
    public DiscordMessageComponentType ComponentType { get; }

    /// <summary>
    /// The values selected in the associated select menu, if applicable.
    /// </summary>
    public Optional<IReadOnlyList<ISelectOption>> Values { get; }
}
