// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents submitted data for a single application command data option during the autocomplete
/// process.
/// </summary>
public interface IAutocompleteInteractionDataOption : IApplicationCommandInteractionDataOption
{
    /// <summary>
    /// Indicates whether this option is currently focused for autocomplete.
    /// </summary>
    public Optional<bool> Focused { get; }
}
