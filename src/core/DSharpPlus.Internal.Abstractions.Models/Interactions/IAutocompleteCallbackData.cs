// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents callback data for responding to an autocomplete interaction.
/// </summary>
public interface IAutocompleteCallbackData
{
    /// <summary>
    /// Up to 25 choices for the end user to choose from.
    /// </summary>
    public IReadOnlyList<IApplicationCommandOptionChoice> Choices { get; }
}
