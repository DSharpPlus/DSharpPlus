// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Contains metadata for a modal submission interaction.
/// </summary>
public interface IModalInteractionData
{
    /// <summary>
    /// The developer-defined ID of this modal.
    /// </summary>
    public string CustomId { get; }

    /// <summary>
    /// The values submitted by the user.
    /// </summary>
    public IReadOnlyList<IComponent> Components { get; }
}
