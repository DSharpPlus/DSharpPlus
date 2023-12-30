// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using OneOf;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents submitted data for a single application command option.
/// </summary>
public interface IApplicationCommandInteractionDataOption
{
    /// <summary>
    /// The name of the option.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of this option.
    /// </summary>
    public DiscordApplicationCommandOptionType Type { get; }

    /// <summary>
    /// The value passed to this option by the user.
    /// </summary>
    public Optional<OneOf<string, int, double, bool>> Value { get; }

    /// <summary>
    /// If this is a subcommand, the values passed to its parameters. If this is a subcommand group,
    /// its subcommands.
    /// </summary>
    public Optional<IReadOnlyList<IApplicationCommandInteractionDataOption>> Options { get; }

    /// <summary>
    /// Indicates whether this option is currently focused for autocomplete.
    /// </summary>
    public Optional<bool> Focused { get; }
}
