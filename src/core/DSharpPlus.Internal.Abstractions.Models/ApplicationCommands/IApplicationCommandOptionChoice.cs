// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using OneOf;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Specifies one choice for a <see cref="IApplicationCommandOption"/>.
/// </summary>
public interface IApplicationCommandOptionChoice
{
    /// <summary>
    /// The name of this choice, 1 to 100 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A localization dictionary for <see cref="Name"/>, with the keys being locales.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; }

    /// <summary>
    /// The value of this choice, up to 100 characters if this is a string.
    /// </summary>
    public OneOf<string, int, double> Value { get; }
}
