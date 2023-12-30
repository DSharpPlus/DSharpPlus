// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using OneOf;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a single option for a chat input application command.
/// </summary>
public interface IApplicationCommandOption
{
    /// <summary>
    /// Signifies the type of this option.
    /// </summary>
    public DiscordApplicationCommandOptionType Type { get; }

    /// <summary>
    /// The name of this option or subcommand, between 1 and 32 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A localization dictionary for <seealso cref="Name"/>, with the keys being locales.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; }

    /// <summary>
    /// The description of this option or subcommand, between 1 and 100 characters.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// A localization dictionary for <seealso cref="Description"/>, with the keys being locales.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> DescriptionLocalizations { get; }

    /// <summary>
    /// Specifies whether this parameter is required or optional, default: optional.
    /// </summary>
    public Optional<bool> Required { get; }

    /// <summary>
    /// If this application command option is of <seealso cref="DiscordApplicationCommandOptionType.String"/>,
    /// <seealso cref="DiscordApplicationCommandOptionType.Integer"/> or 
    /// <seealso cref="DiscordApplicationCommandOptionType.Number"/>, up to 25 options to choose from.
    /// These options will be the only valid options for this command.
    /// </summary>
    public Optional<IReadOnlyList<IApplicationCommandOptionChoice>> Choices { get; }

    /// <summary>
    /// If this option is of <seealso cref="DiscordApplicationCommandOptionType.SubCommand"/> or
    /// <seealso cref="DiscordApplicationCommandOptionType.SubCommandGroup"/>, these options will be the
    /// parameters (or subcommands if this is a subcommand group).
    /// </summary>
    public Optional<IReadOnlyList<IApplicationCommandOption>> Options { get; }

    /// <summary>
    /// If this option is of <seealso cref="DiscordApplicationCommandOptionType.Channel"/>, shown
    /// channels will be restricted to these types.
    /// </summary>
    public Optional<IReadOnlyList<DiscordChannelType>> ChannelTypes { get; }

    /// <summary>
    /// If this option is of <seealso cref="DiscordApplicationCommandOptionType.Integer"/> or
    /// <seealso cref="DiscordApplicationCommandOptionType.Number"/>, the minimum value permitted.
    /// </summary>
    public Optional<OneOf<int, double>> MinValue { get; }

    /// <summary>
    /// If this option is of <seealso cref="DiscordApplicationCommandOptionType.Integer"/> or
    /// <seealso cref="DiscordApplicationCommandOptionType.Number"/>, the maximum value permitted.
    /// </summary>
    public Optional<OneOf<int, double>> MaxValue { get; }

    /// <summary>
    /// If this option is of <seealso cref="DiscordApplicationCommandOptionType.String"/>, the minimum
    /// length permitted, between 0 and 6000.
    /// </summary>
    public Optional<int> MinLength { get; }

    /// <summary>
    /// If this option is of <seealso cref="DiscordApplicationCommandOptionType.String"/>, the maximum
    /// length permitted, between 1 and 6000.
    /// </summary>
    public Optional<int> MaxLength { get; }

    /// <summary>
    /// Indicates whether this option is subject to autocomplete. This is mutually exclusive with
    /// <seealso cref="Choices"/> being defined.
    /// </summary>
    public Optional<bool> Autocomplete { get; }
}
