// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents callback data for creating a modal.
/// </summary>
public interface IModalCallbackData
{
    /// <summary>
    /// The developer-defined custom identifier for this modal, up to 100 characters.
    /// </summary>
    public string CustomId { get; }

    /// <summary>
    /// The title of the modal, up to 45 characters.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Between 1 and 5 action rows containing each one text input component.
    /// </summary>
    public IReadOnlyList<IActionRowComponent> Components { get; }
}
